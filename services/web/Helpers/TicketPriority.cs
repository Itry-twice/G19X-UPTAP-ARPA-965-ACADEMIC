namespace PluriOne.Web.Helpers;

/// <summary>Traduce la prioridad de los tickets a español.</summary>
public static class TicketPriority
{
    public static string Label(string? priority) => priority switch
    {
        "low" => "Baja",
        "normal" => "Normal",
        "high" => "Alta",
        _ => priority ?? string.Empty
    };
}
