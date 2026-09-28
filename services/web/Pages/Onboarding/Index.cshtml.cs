using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PluriOne.Shared.Auth;
using PluriOne.Web.Contracts;
using PluriOne.Web.Services;

namespace PluriOne.Web.Pages.Onboarding;

public sealed class IndexModel : PageModel
{
    private readonly Gateway _gateway;
    private readonly ICurrentUserContext _currentUser;

    public IndexModel(Gateway gateway, ICurrentUserContext currentUser)
    {
        _gateway = gateway;
        _currentUser = currentUser;
    }

    public List<TaskItemDto> Tasks { get; set; } = new();
    public ProgressDto? Progress { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!_currentUser.User?.IsColaborador == true)
        {
            return Forbid();
        }

        Tasks = await _gateway.GetTasksAsync();
        Progress = await _gateway.GetProgressAsync();
        return Page();
    }
}
