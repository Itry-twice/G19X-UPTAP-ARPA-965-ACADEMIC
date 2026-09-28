using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PluriOne.Shared.Auth;
using PluriOne.Web.Services;

namespace PluriOne.Web.Pages.Profile;

public sealed class IndexModel : PageModel
{
    private readonly Gateway _gateway;
    private readonly ICurrentUserContext _currentUser;

    public IndexModel(Gateway gateway, ICurrentUserContext currentUser)
    {
        _gateway = gateway;
        _currentUser = currentUser;
    }

    public CurrentUser? AccountUser => _currentUser.User;
    public string? Message { get; set; }
    public string? Error { get; set; }

    [BindProperty]
    public string FullName { get; set; } = string.Empty;

    [BindProperty]
    public string CurrentPassword { get; set; } = string.Empty;

    [BindProperty]
    public string NewPassword { get; set; } = string.Empty;

    [BindProperty]
    public string ConfirmPassword { get; set; } = string.Empty;

    public void OnGet() { }

    public async Task<IActionResult> OnPostUpdateNameAsync()
    {
        if (string.IsNullOrWhiteSpace(FullName))
        {
            Error = "El nombre no puede estar vacío.";
            return Page();
        }

        var ok = await _gateway.UpdateFullNameAsync(FullName.Trim());
        Message = ok ? "Nombre actualizado. Se reflejará al volver a iniciar sesión." : null;
        Error = ok ? null : "No fue posible actualizar el nombre.";
        return Page();
    }

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword))
        {
            Error = "Completa la contraseña actual y la nueva.";
            return Page();
        }

        if (NewPassword != ConfirmPassword)
        {
            Error = "Las contraseñas no coinciden.";
            return Page();
        }

        var (ok, error) = await _gateway.ChangePasswordAsync(CurrentPassword, NewPassword);
        if (!ok)
        {
            Error = error switch
            {
                "invalid_current" => "La contraseña actual es incorrecta.",
                "weak_password" => "La nueva contraseña debe tener al menos 8 caracteres.",
                _ => "No fue posible cambiar la contraseña."
            };
            return Page();
        }

        Message = "Contraseña actualizada correctamente.";
        return Page();
    }
}
