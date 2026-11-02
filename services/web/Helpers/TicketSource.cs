namespace PluriOne.Web.Helpers;

/// <summary>Traduce el origen de un ticket a español.</summary>
public static class TicketSource
{
    public static string Label(string? source) => source switch
    {
        "manual" => "Manual",
        "assistant" => "Asistente virtual",
        _ => source ?? string.Empty
    };
}
