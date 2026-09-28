using Microsoft.AspNetCore.Mvc.RazorPages;
using PluriOne.Shared.Auth;
using PluriOne.Web.Contracts;
using PluriOne.Web.Services;

namespace PluriOne.Web.Pages;

public sealed class IndexModel : PageModel
{
    private readonly Gateway _gateway;
    private readonly ICurrentUserContext _currentUser;

    public IndexModel(Gateway gateway, ICurrentUserContext currentUser)
    {
        _gateway = gateway;
        _currentUser = currentUser;
    }

    public string? Greeting { get; set; }
    public bool IsColaborador { get; set; }
    public bool IsRrhh { get; set; }
    public bool IsAdmin { get; set; }

    // Colaborador
    public EmployeeDto? Employee { get; set; }
    public ProgressDto? Progress { get; set; }
    public List<TicketDto> RecentTickets { get; set; } = new();

    // RRHH
    public int OpenTickets { get; set; }
    public int TotalTickets { get; set; }

    // Admin
    public int UserCount { get; set; }
    public int AuditCount { get; set; }

    public async Task OnGetAsync()
    {
        var user = _currentUser.User;
        IsColaborador = user?.IsColaborador == true;
        IsRrhh = user?.IsRrhh == true;
        IsAdmin = user?.IsAdmin == true;

        var hour = DateTime.Now.Hour;
        Greeting = hour < 12 ? "Buenos días" : hour < 19 ? "Buenas tardes" : "Buenas noches";

        if (IsColaborador)
        {
            Employee = await _gateway.GetEmployeeAsync();
            Progress = await _gateway.GetProgressAsync();
            var tickets = await _gateway.GetTicketsAsync();
            RecentTickets = tickets.OrderByDescending(t => t.CreatedAt).Take(3).ToList();
        }
        else if (IsRrhh)
        {
            var tickets = await _gateway.GetTicketsAsync();
            TotalTickets = tickets.Count;
            OpenTickets = tickets.Count(t => t.Status is "open" or "in_progress");
        }
        else if (IsAdmin)
        {
            UserCount = (await _gateway.GetUsersAsync()).Count;
            AuditCount = (await _gateway.GetAuditAsync()).Count;
        }
    }
}
