using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PluriOne.Shared.Auth;
using PluriOne.Web.Contracts;
using PluriOne.Web.Services;

namespace PluriOne.Web.Pages.Tickets;

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
    public List<CategoryDto> Categories { get; set; } = new();
    public bool CanCreate => _currentUser.User?.IsColaborador == true;
    public string Kicker => _currentUser.User?.IsRrhh == true ? "Recursos Humanos" : "Colaborador";

    [BindProperty]
    public string Subject { get; set; } = string.Empty;

    [BindProperty]
    public string? Description { get; set; }

    [BindProperty]
    public Guid? CategoryId { get; set; }

    [BindProperty]
    public string Priority { get; set; } = "normal";

    public string? Error { get; set; }

    public async Task OnGetAsync()
    {
        Tickets = await _gateway.GetTicketsAsync();
        Categories = await _gateway.GetCategoriesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!CanCreate)
        {
            return Forbid();
        }

        Categories = await _gateway.GetCategoriesAsync();

        if (string.IsNullOrWhiteSpace(Subject))
        {
            Error = "El asunto es obligatorio.";
            Tickets = await _gateway.GetTicketsAsync();
            return Page();
        }

        var (ticket, error) = await _gateway.CreateTicketAsync(Subject.Trim(), Description, CategoryId, Priority);
        if (ticket is null)
        {
            Error = "No fue posible crear la solicitud.";
            Tickets = await _gateway.GetTicketsAsync();
            return Page();
        }

        return RedirectToPage("/Tickets/Detail", new { id = ticket.Id });
    }
}
