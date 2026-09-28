namespace PluriOne.Shared.Auth;

public sealed record CurrentUser(Guid Id, string Email, string FullName, string Role)
{
    public bool IsStaff => Role is "rrhh" or "admin";
    public bool IsAdmin => Role is "admin";
    public bool IsRrhh => Role is "rrhh";
    public bool IsColaborador => Role is "colaborador";
}
