using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PluriOne.Shared.Auth;
using PluriOne.Web.Contracts;
using PluriOne.Web.Services;

namespace PluriOne.Web.Pages.Plan;

public sealed class IndexModel : PageModel
{
    private readonly Gateway _gateway;
    private readonly ICurrentUserContext _currentUser;

    public IndexModel(Gateway gateway, ICurrentUserContext currentUser)
    {
        _gateway = gateway;
        _currentUser = currentUser;
    }

    public List<PlanPhaseDto> Phases { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!_currentUser.User?.IsColaborador == true)
        {
            return Forbid();
        }

        Phases = await _gateway.GetPlanAsync();
        return Page();
    }
}
