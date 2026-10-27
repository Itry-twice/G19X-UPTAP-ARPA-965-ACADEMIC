using PluriOne.Web.Helpers;
using Xunit;

namespace PluriOne.Tests;

public class HelpersTests
{
    [Theory]
    [InlineData("pending", "Pendiente")]
    [InlineData("in_progress", "En proceso")]
    [InlineData("completed", "Completada")]
    [InlineData("otro_valor", "otro_valor")]
    [InlineData(null, "")]
    public void TaskItemStatus_traduce(string? valor, string esperado)
        => Assert.Equal(esperado, TaskItemStatus.Label(valor));

    [Theory]
    [InlineData("low", "Baja")]
    [InlineData("normal", "Normal")]
    [InlineData("high", "Alta")]
    [InlineData("otro_valor", "otro_valor")]
    [InlineData(null, "")]
    public void TicketPriority_traduce(string? valor, string esperado)
        => Assert.Equal(esperado, TicketPriority.Label(valor));

    [Theory]
    [InlineData("colaborador", "Colaborador")]
    [InlineData("rrhh", "Recursos Humanos")]
    [InlineData("admin", "Administrador")]
    [InlineData("otro_valor", "otro_valor")]
    [InlineData(null, "")]
    public void UserRole_traduce(string? valor, string esperado)
        => Assert.Equal(esperado, UserRole.Label(valor));

    [Theory]
    [InlineData("open", "Abierta")]
    [InlineData("in_progress", "En proceso")]
    [InlineData("resolved", "Resuelta")]
    [InlineData("closed", "Cerrada")]
    [InlineData("otro_valor", "otro_valor")]
    [InlineData(null, "")]
    public void TicketStatus_traduce(string? valor, string esperado)
        => Assert.Equal(esperado, TicketStatus.Label(valor));
}
