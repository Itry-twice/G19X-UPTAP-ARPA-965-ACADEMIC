using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PluriOne.Web.Services;

namespace PluriOne.Web.Pages.Account;

public sealed class LogoutModel : PageModel
{
    private readonly AuthFlowService _authFlow;

    public LogoutModel(AuthFlowService authFlow) => _authFlow = authFlow;

    public async Task<IActionResult> OnPostAsync()
    {
        await _authFlow.LogoutAsync();
        return RedirectToPage("/Account/Login");
    }
}
