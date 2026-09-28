using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PluriOne.Web.Services;

namespace PluriOne.Web.Pages.Account;

public sealed class LoginModel : PageModel
{
    private readonly AuthFlowService _authFlow;

    public LoginModel(AuthFlowService authFlow) => _authFlow = authFlow;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? Error { get; set; }

    public void OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            // ya autenticado: no mostrar login
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            Error = "Ingresa tu correo y contraseña.";
            return Page();
        }

        var result = await _authFlow.LoginAsync(Email.Trim(), Password);

        switch (result)
        {
            case LoginResult.Success:
                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    return LocalRedirect(ReturnUrl);
                }
                return RedirectToPage("/Index");

            case LoginResult.InvalidCredentials:
                Error = "Correo o contraseña incorrectos.";
                return Page();

            case LoginResult.LockedOut:
                Error = "Cuenta temporalmente bloqueada por intentos fallidos.";
                return Page();

            default:
                Error = "No fue posible iniciar sesión. Intenta de nuevo.";
                return Page();
        }
    }
}
