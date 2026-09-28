using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PluriOne.Shared.Auth;
using PluriOne.Web.Contracts;
using PluriOne.Web.Services;

namespace PluriOne.Web.Pages.HR;

public sealed class IndexModel : PageModel
{
    private readonly Gateway _gateway;
    private readonly ICurrentUserContext _currentUser;

    public IndexModel(Gateway gateway, ICurrentUserContext currentUser)
    {
        _gateway = gateway;
        _currentUser = currentUser;
    }

    public List<TicketDto> Tickets { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty]
    public string NewStatus { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        if (!_currentUser.User?.IsRrhh == true)
        {
            return Forbid();
        }

        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostStatusAsync(Guid id)
    {
        if (!_currentUser.User?.IsRrhh == true)
        {
            return Forbid();
        }

        await _gateway.UpdateTicketStatusAsync(id, NewStatus, null);
        return RedirectToPage(new { StatusFilter });
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        if (!_currentUser.User?.IsRrhh == true)
        {
            return Forbid();
        }

        await _gateway.DeleteTicketAsync(id);
        return RedirectToPage(new { StatusFilter });
    }

    private async Task LoadAsync()
    {
        var all = await _gateway.GetTicketsAsync();
        if (!string.IsNullOrEmpty(StatusFilter))
        {
            all = all.Where(t => t.Status == StatusFilter).ToList();
        }
        Tickets = all;
    }
}
