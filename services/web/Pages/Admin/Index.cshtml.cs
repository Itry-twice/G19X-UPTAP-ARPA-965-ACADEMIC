using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PluriOne.Shared.Auth;
using PluriOne.Web.Contracts;
using PluriOne.Web.Services;

namespace PluriOne.Web.Pages.Admin;

public sealed class IndexModel : PageModel
{
    private readonly Gateway _gateway;
    private readonly ICurrentUserContext _currentUser;

    public IndexModel(Gateway gateway, ICurrentUserContext currentUser)
    {
        _gateway = gateway;
        _currentUser = currentUser;
    }

    public List<AdminUserDto> Users { get; set; } = new();
    public List<TicketDto> EscalatedTickets { get; set; } = new();
    public List<DocumentDto> Documents { get; set; } = new();
    public string? Message { get; set; }

    public int DisplayYear { get; set; }
    public int DisplayMonth { get; set; }
    public DateOnly? SelectedDate { get; set; }
    public List<CalendarDay> CalendarDays { get; set; } = new();
    public List<AuditEventDto> DayEvents { get; set; } = new();

    public string MonthName => new DateTime(DisplayYear, DisplayMonth, 1).ToString("MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("es-MX"));

    [BindProperty]
    public string NewRole { get; set; } = string.Empty;

    [BindProperty]
    public string NewEmail { get; set; } = string.Empty;

    [BindProperty]
    public string NewFullName { get; set; } = string.Empty;

    [BindProperty]
    public string NewUserRole { get; set; } = "colaborador";

    [BindProperty]
    public string NewPassword { get; set; } = string.Empty;

    [BindProperty]
    public string? NewEmployeeCode { get; set; }

    [BindProperty]
    public string ResetPassword { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? Date { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Year { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Month { get; set; }

    public sealed record CalendarDay(int? Day, string? Date, bool HasActivity, bool IsSelected);

    public async Task<IActionResult> OnGetAsync()
    {
        if (!_currentUser.User?.IsAdmin == true)
        {
            return Forbid();
        }

        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCreateUserAsync()
    {
        if (!_currentUser.User?.IsAdmin == true) return Forbid();
        var (ok, error) = await _gateway.CreateUserAsync(NewEmail, NewFullName, NewUserRole, NewPassword, NewEmployeeCode);
        Message = ok ? "Usuario creado correctamente." : (error == "email_exists" ? "Ya existe un usuario con ese correo." : "No fue posible crear el usuario.");
        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostRoleAsync(Guid id)
    {
        if (!_currentUser.User?.IsAdmin == true) return Forbid();
        var (ok, error) = await _gateway.UpdateUserRoleAsync(id, NewRole);
        Message = ok ? null : (error == "cannot_change_own_role" ? "No puedes cambiar tu propio rol." : "No fue posible cambiar el rol.");
        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostStatusAsync(Guid id, bool isActive)
    {
        if (!_currentUser.User?.IsAdmin == true) return Forbid();
        var (ok, error) = await _gateway.UpdateUserStatusAsync(id, isActive);
        Message = ok ? null : "No fue posible actualizar el usuario.";
        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostResetPasswordAsync(Guid id)
    {
        if (!_currentUser.User?.IsAdmin == true) return Forbid();
        var (ok, error) = await _gateway.ResetUserPasswordAsync(id, ResetPassword);
        Message = ok ? "Contraseña reiniciada correctamente." : (error == "weak_password" ? "La contraseña debe tener al menos 8 caracteres." : "No fue posible reiniciar la contraseña.");
        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostIngestAsync()
    {
        if (!_currentUser.User?.IsAdmin == true) return Forbid();
        var (ok, result) = await _gateway.IngestKnowledgeBaseAsync();
        Message = ok ? "Base de conocimiento actualizada." : "No fue posible actualizar la base de conocimiento.";
        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteTicketAsync(Guid id)
    {
        if (!_currentUser.User?.IsAdmin == true) return Forbid();
        await _gateway.DeleteTicketAsync(id);
        Message = "Solicitud eliminada.";
        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteDocumentAsync(string id)
    {
        if (!_currentUser.User?.IsAdmin == true) return Forbid();
        var (ok, error) = await _gateway.DeleteDocumentAsync(id);
        Message = ok ? "Documento eliminado." : "No fue posible eliminar el documento.";
        await LoadAsync();
        return Page();
    }

    private async Task LoadAsync()
    {
        DateOnly? selected = null;
        if (DateOnly.TryParseExact(Date, "yyyy-MM-dd", out var parsed))
        {
            selected = parsed;
        }

        if (Year.HasValue && Month is >= 1 and <= 12)
        {
            DisplayYear = Year.Value;
            DisplayMonth = Month.Value;
        }
        else if (selected.HasValue)
        {
            DisplayYear = selected.Value.Year;
            DisplayMonth = selected.Value.Month;
        }
        else
        {
            DisplayYear = DateTime.Now.Year;
            DisplayMonth = DateTime.Now.Month;
        }

        SelectedDate = selected;

        Users = await _gateway.GetUsersAsync();
        EscalatedTickets = await _gateway.GetEscalatedTicketsAsync();
        Documents = await _gateway.GetDocumentsAsync();

        var summary = await _gateway.GetAuditSummaryAsync(DisplayYear, DisplayMonth);
        var activeDates = new HashSet<string>(summary.Select(s => s.Date));
        CalendarDays = BuildCalendar(DisplayYear, DisplayMonth, activeDates, SelectedDate);

        if (SelectedDate.HasValue)
        {
            var day = SelectedDate.Value.ToDateTime(TimeOnly.MinValue);
            DayEvents = await _gateway.GetAuditByDateAsync(day);
        }
        else
        {
            DayEvents = new List<AuditEventDto>();
        }
    }

    private static List<CalendarDay> BuildCalendar(int year, int month, HashSet<string> activeDates, DateOnly? selected)
    {
        var first = new DateOnly(year, month, 1);
        var offset = ((int)first.DayOfWeek + 6) % 7; // lunes = 0
        var daysInMonth = DateTime.DaysInMonth(year, month);

        var cells = new List<CalendarDay>();
        for (var i = 0; i < offset; i++)
        {
            cells.Add(new CalendarDay(null, null, false, false));
        }

        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateOnly(year, month, day);
            var key = date.ToString("yyyy-MM-dd");
            cells.Add(new CalendarDay(day, key, activeDates.Contains(key), selected == date));
        }

        return cells;
    }

    public static string FriendlyAction(string action) => action switch
    {
        "login.success" => "Inició sesión",
        "login.failed" => "Intento de acceso fallido",
        "logout" => "Cerró sesión",
        "token.refresh" => "Renovó sesión",
        "password.changed" => "Cambió su contraseña",
        _ => action
    };
}
