using Microsoft.EntityFrameworkCore;
using PluriOne.HrService.Data;
using PluriOne.HrService.Dtos;
using PluriOne.HrService.Services;
using PluriOne.Shared.Auth;
using Xunit;

namespace PluriOne.Tests;

public sealed class HrManagerTests
{
    private static HrDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<HrDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new HrDbContext(options);
    }

    private static CurrentUser Colaborador() =>
        new(Guid.Parse("33333333-3333-3333-3333-333333333333"), "c@example.com", "C", "colaborador");

    private static CurrentUser Rrhh() =>
        new(Guid.Parse("22222222-2222-2222-2222-222222222222"), "r@example.com", "R", "rrhh");

    private static CurrentUser OtherColaborador() =>
        new(Guid.Parse("44444444-4444-4444-4444-444444444444"), "o@example.com", "O", "colaborador");

    [Fact]
    public async Task CreateTicket_SetsRequesterToCurrentUser()
    {
        await using var db = CreateDb();
        var manager = new HrManager(db);

        var (ticket, error) = await manager.CreateTicketAsync(Colaborador(), new CreateTicketRequest("Sin acceso al correo", "no puedo entrar", null, "normal", null));

        Assert.Null(error);
        Assert.NotNull(ticket);
        Assert.Equal(Colaborador().Id, ticket!.RequesterUserId);
        Assert.Equal("open", ticket.Status);
    }

    [Fact]
    public async Task CreateTicket_RejectsInvalidPriority()
    {
        await using var db = CreateDb();
        var manager = new HrManager(db);

        var (ticket, error) = await manager.CreateTicketAsync(Colaborador(), new CreateTicketRequest("x", null, null, "urgent", null));

        Assert.Null(ticket);
        Assert.Equal("invalid_priority", error);
    }

    [Fact]
    public async Task ListTickets_ColaboradorSeesOnlyOwnTickets()
    {
        await using var db = CreateDb();
        var manager = new HrManager(db);

        await manager.CreateTicketAsync(Colaborador(), new CreateTicketRequest("ticket propio", null, null, "normal", null));
        await manager.CreateTicketAsync(OtherColaborador(), new CreateTicketRequest("ticket ajeno", null, null, "normal", null));

        var tickets = await manager.ListTicketsAsync(Colaborador());

        Assert.Single(tickets);
        Assert.Equal("ticket propio", tickets[0].Subject);
    }

    [Fact]
    public async Task ListTickets_RrhhSeesAllTickets()
    {
        await using var db = CreateDb();
        var manager = new HrManager(db);

        await manager.CreateTicketAsync(Colaborador(), new CreateTicketRequest("a", null, null, "normal", null));
        await manager.CreateTicketAsync(OtherColaborador(), new CreateTicketRequest("b", null, null, "normal", null));

        var tickets = await manager.ListTicketsAsync(Rrhh());

        Assert.Equal(2, tickets.Count);
    }

    [Fact]
    public async Task UpdateStatus_ColaboradorIsForbidden()
    {
        await using var db = CreateDb();
        var manager = new HrManager(db);

        var (ticket, _) = await manager.CreateTicketAsync(Colaborador(), new CreateTicketRequest("t", null, null, "normal", null));
        var (ok, error) = await manager.UpdateStatusAsync(Colaborador(), ticket!.Id, "closed", null);

        Assert.False(ok);
        Assert.Equal("forbidden", error);
    }

    [Fact]
    public async Task UpdateStatus_RrhhSucceeds_AndRecordsHistory()
    {
        await using var db = CreateDb();
        var manager = new HrManager(db);

        var (ticket, _) = await manager.CreateTicketAsync(Colaborador(), new CreateTicketRequest("t", null, null, "normal", null));
        var (ok, error) = await manager.UpdateStatusAsync(Rrhh(), ticket!.Id, "in_progress", null);

        Assert.True(ok);
        Assert.Null(error);

        var history = await db.TicketStatusHistory.CountAsync(h => h.TicketId == ticket.Id);
        Assert.Equal(2, history); // open + in_progress
    }

    [Fact]
    public async Task AddComment_NonOwnerColaboradorCannotComment()
    {
        await using var db = CreateDb();
        var manager = new HrManager(db);

        var (ticket, _) = await manager.CreateTicketAsync(Colaborador(), new CreateTicketRequest("t", null, null, "normal", null));
        var ok = await manager.AddCommentAsync(OtherColaborador(), ticket!.Id, "intrusión");

        Assert.False(ok);
    }
}
