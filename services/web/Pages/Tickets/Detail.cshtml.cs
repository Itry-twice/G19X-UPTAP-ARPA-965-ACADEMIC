using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PluriOne.Shared.Auth;
using PluriOne.Web.Contracts;
using PluriOne.Web.Services;

namespace PluriOne.Web.Pages.Tickets;

public sealed class DetailModel : PageModel
{
    private readonly Gateway _gateway;
    private readonly ICurrentUserContext _currentUser;

    public DetailModel(Gateway gateway, ICurrentUserContext currentUser)
    {
        _gateway = gateway;
        _currentUser = currentUser;
    }

    public TicketDetailDto? Ticket { get; set; }
    public bool IsStaff => _currentUser.User?.IsRrhh == true;

    [BindProperty]
    public string CommentBody { get; set; } = string.Empty;

    [BindProperty]
    public string NewStatus { get; set; } = string.Empty;

    public string? Error { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Ticket = await _gateway.GetTicketAsync(id);
        if (Ticket is null)
        {
            return NotFound();
        }
        return Page();
    }

    public async Task<IActionResult> OnPostCommentAsync(Guid id)
    {
        if (string.IsNullOrWhiteSpace(CommentBody))
        {
            Error = "El comentario no puede estar vacío.";
            Ticket = await _gateway.GetTicketAsync(id);
            return Page();
        }

        await _gateway.AddCommentAsync(id, CommentBody.Trim());
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostStatusAsync(Guid id)
    {
        if (!IsStaff)
        {
            return Forbid();
        }

        var (ok, error) = await _gateway.UpdateTicketStatusAsync(id, NewStatus, null);
        if (!ok)
        {
            Error = "No fue posible actualizar el estado.";
        }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostEscalateAsync(Guid id)
    {
        if (!IsStaff)
        {
            return Forbid();
        }

        var (ok, error) = await _gateway.EscalateTicketAsync(id);
        if (!ok)
        {
            Error = "No fue posible escalar la solicitud.";
        }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        if (!IsStaff)
        {
            return Forbid();
        }

        var (ok, error) = await _gateway.DeleteTicketAsync(id);
        if (!ok)
        {
            Error = "Solo se pueden eliminar solicitudes resueltas o cerradas.";
            return RedirectToPage(new { id });
        }

        return RedirectToPage("/Tickets/Index");
    }
}
