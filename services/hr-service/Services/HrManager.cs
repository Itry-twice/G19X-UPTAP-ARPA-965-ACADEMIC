using Microsoft.EntityFrameworkCore;
using PluriOne.HrService.Data;
using PluriOne.HrService.Dtos;
using PluriOne.Shared.Auth;

namespace PluriOne.HrService.Services;

public sealed class HrManager
{
    private static readonly string[] AllowedStatuses = { "open", "in_progress", "resolved", "closed" };
    private static readonly string[] AllowedPriorities = { "low", "normal", "high" };
    private static readonly string[] AllowedSources = { "manual", "assistant" };

    private readonly HrDbContext _db;

    public HrManager(HrDbContext db) => _db = db;

    public async Task<List<CategoryDto>> ListCategoriesAsync()
    {
        return await _db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description))
            .ToListAsync();
    }

    public async Task<List<TicketDto>> ListTicketsAsync(CurrentUser user)
    {
        var query = _db.Tickets.AsNoTracking();

        // Defensa en profundidad además de RLS.
        if (!user.IsStaff)
        {
            query = query.Where(t => t.RequesterUserId == user.Id);
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TicketDto(
                t.Id, t.Subject, t.Description, t.CategoryId,
                t.Category != null ? t.Category.Name : null,
                t.RequesterUserId, t.RequesterName, t.AssigneeUserId, t.Status, t.Priority, t.Source,
                t.Escalated, t.CreatedAt, t.UpdatedAt))
            .ToListAsync();
    }

    public async Task<TicketDetailDto?> GetTicketAsync(Guid id, CurrentUser user)
    {
        var ticket = await _db.Tickets
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket is null)
        {
            return null;
        }

        if (!user.IsStaff && ticket.RequesterUserId != user.Id)
        {
            return null;
        }

        var comments = await _db.TicketComments
            .Where(c => c.TicketId == id)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto(c.Id, c.AuthorUserId, c.AuthorName, c.Body, c.CreatedAt))
            .ToListAsync();

        var history = await _db.TicketStatusHistory
            .Where(h => h.TicketId == id)
            .OrderBy(h => h.ChangedAt)
            .Select(h => new StatusHistoryDto(h.FromStatus, h.ToStatus, h.ChangedAt))
            .ToListAsync();

        return new TicketDetailDto(
            ticket.Id, ticket.Subject, ticket.Description, ticket.CategoryId, ticket.Category?.Name,
            ticket.RequesterUserId, ticket.RequesterName, ticket.AssigneeUserId, ticket.Status, ticket.Priority, ticket.Source,
            ticket.Escalated, ticket.CreatedAt, ticket.UpdatedAt, comments, history);
    }

    public async Task<(Ticket? Ticket, string? Error)> CreateTicketAsync(CurrentUser user, CreateTicketRequest request)
    {
        if (!user.IsColaborador)
        {
            return (null, "forbidden");
        }

        if (string.IsNullOrWhiteSpace(request.Subject))
        {
            return (null, "subject_required");
        }

        var priority = string.IsNullOrWhiteSpace(request.Priority) ? "normal" : request.Priority;
        if (!AllowedPriorities.Contains(priority))
        {
            return (null, "invalid_priority");
        }

        var source = string.IsNullOrWhiteSpace(request.Source) ? "manual" : request.Source;
        if (!AllowedSources.Contains(source))
        {
            source = "manual";
        }

        if (request.CategoryId.HasValue &&
            !await _db.Categories.AnyAsync(c => c.Id == request.CategoryId.Value))
        {
            return (null, "invalid_category");
        }

        var now = DateTimeOffset.UtcNow;
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Subject = request.Subject.Trim(),
            Description = request.Description,
            CategoryId = request.CategoryId,
            RequesterUserId = user.Id,
            RequesterName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName,
            Status = "open",
            Priority = priority,
            Source = source,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Tickets.Add(ticket);
        _db.TicketStatusHistory.Add(new TicketStatusHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            FromStatus = null,
            ToStatus = "open",
            ChangedByUserId = user.Id,
            ChangedAt = now
        });

        await _db.SaveChangesAsync();
        return (ticket, null);
    }

    public async Task<bool> AddCommentAsync(CurrentUser user, Guid ticketId, string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return false;
        }

        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket is null)
        {
            return false;
        }

        if (!user.IsStaff && ticket.RequesterUserId != user.Id)
        {
            return false;
        }

        _db.TicketComments.Add(new TicketComment
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            AuthorUserId = user.Id,
            AuthorName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName,
            Body = body.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Ok, string? Error)> UpdateStatusAsync(CurrentUser user, Guid ticketId, string status, Guid? assigneeUserId)
    {
        if (!user.IsRrhh)
        {
            return (false, "forbidden");
        }

        if (!AllowedStatuses.Contains(status))
        {
            return (false, "invalid_status");
        }

        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket is null)
        {
            return (false, "not_found");
        }

        if (assigneeUserId.HasValue && assigneeUserId.Value != ticket.AssigneeUserId)
        {
            ticket.AssigneeUserId = assigneeUserId;
        }

        if (ticket.Status != status)
        {
            _db.TicketStatusHistory.Add(new TicketStatusHistory
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                FromStatus = ticket.Status,
                ToStatus = status,
                ChangedByUserId = user.Id,
                ChangedAt = DateTimeOffset.UtcNow
            });
            ticket.Status = status;
        }

        ticket.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> EscalateAsync(CurrentUser user, Guid ticketId)
    {
        if (!user.IsRrhh)
        {
            return (false, "forbidden");
        }

        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket is null)
        {
            return (false, "not_found");
        }

        ticket.Escalated = true;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<List<TicketDto>> ListEscalatedAsync()
    {
        return await _db.Tickets
            .Where(t => t.Escalated)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TicketDto(
                t.Id, t.Subject, t.Description, t.CategoryId,
                t.Category != null ? t.Category.Name : null,
                t.RequesterUserId, t.RequesterName, t.AssigneeUserId, t.Status, t.Priority, t.Source,
                t.Escalated, t.CreatedAt, t.UpdatedAt))
            .ToListAsync();
    }

    public async Task<(bool Ok, string? Error)> DeleteAsync(CurrentUser user, Guid ticketId)
    {
        if (!user.IsRrhh && !user.IsAdmin)
        {
            return (false, "forbidden");
        }

        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket is null)
        {
            return (false, "not_found");
        }

        if (ticket.Status is not "resolved" and not "closed")
        {
            return (false, "not_resolved");
        }

        _db.Tickets.Remove(ticket);
        await _db.SaveChangesAsync();
        return (true, null);
    }
}
