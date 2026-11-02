namespace PluriOne.Web.Helpers;

/// <summary>Traduce la categoría de una tarea del checklist a español.</summary>
public static class TaskCategory
{
    public static string Label(string? category) => category switch
    {
        "setup" => "Configuración",
        "compliance" => "Cumplimiento",
        "onboarding" => "Integración",
        "general" => "General",
        _ => category ?? string.Empty
    };
}
