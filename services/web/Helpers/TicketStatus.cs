namespace PluriOne.Web.Helpers;

/// <summary>Traduce los estados de los tickets a etiquetas en español para la interfaz.</summary>
public static class TicketStatus
{
    public static string Label(string? status) => status switch
    {
        "open" => "Abierta",
        "in_progress" => "En proceso",
        "resolved" => "Resuelta",
        "closed" => "Cerrada",
        _ => status ?? string.Empty
    };
}
