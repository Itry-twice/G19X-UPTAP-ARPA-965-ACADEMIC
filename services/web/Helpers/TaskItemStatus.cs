namespace PluriOne.Web.Helpers;

/// <summary>Traduce el estado de las tareas de onboarding a español.</summary>
public static class TaskItemStatus
{
    public static string Label(string? status) => status switch
    {
        "pending" => "Pendiente",
        "in_progress" => "En proceso",
        "completed" => "Completada",
        _ => status ?? string.Empty
    };
}
