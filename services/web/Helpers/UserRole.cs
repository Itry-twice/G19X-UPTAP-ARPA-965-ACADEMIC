namespace PluriOne.Web.Helpers;

/// <summary>Traduce el rol del usuario a español.</summary>
public static class UserRole
{
    public static string Label(string? role) => role switch
    {
        "colaborador" => "Colaborador",
        "rrhh" => "Recursos Humanos",
        "admin" => "Administrador",
        _ => role ?? string.Empty
    };
}
