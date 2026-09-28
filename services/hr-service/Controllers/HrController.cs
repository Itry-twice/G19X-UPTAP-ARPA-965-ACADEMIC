using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PluriOne.HrService.Dtos;
using PluriOne.HrService.Services;
using PluriOne.Shared.Auth;
using PluriOne.Shared.Contracts;

namespace PluriOne.HrService.Controllers;

[ApiController]
[Authorize]
[Route("api/hr")]
public sealed class HrController : ControllerBase
{
    private readonly HrManager _manager;
    private readonly ICurrentUserContext _currentUser;

    public HrController(HrManager manager, ICurrentUserContext currentUser)
    {
        _manager = manager;
        _currentUser = currentUser;
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Categories()
    {
        return Ok(await _manager.ListCategoriesAsync());
    }

    [HttpGet("tickets")]
    public async Task<IActionResult> Tickets()
    {
        var user = RequireUser();
        if (user is null) return Unauthorized(new ApiError("unauthorized", "Autenticación requerida."));

        return Ok(await _manager.ListTicketsAsync(user));
    }

    [HttpGet("tickets/{id:guid}")]
    public async Task<IActionResult> GetTicket(Guid id)
    {
        var user = RequireUser();
        if (user is null) return Unauthorized(new ApiError("unauthorized", "Autenticación requerida."));

        var ticket = await _manager.GetTicketAsync(id, user);
        return ticket is null
            ? NotFound(new ApiError("not_found", "Ticket no encontrado."))
            : Ok(ticket);
    }

    [HttpPost("tickets")]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
    {
        var user = RequireUser();
        if (user is null) return Unauthorized(new ApiError("unauthorized", "Autenticación requerida."));

        var (ticket, error) = await _manager.CreateTicketAsync(user, request);
        if (ticket is null)
        {
            return error switch
            {
                "forbidden" => StatusCode(StatusCodes.Status403Forbidden,
                    new ApiError("forbidden", "Solo los colaboradores pueden crear solicitudes.")),
                "subject_required" => BadRequest(new ApiError("subject_required", "El asunto es obligatorio.")),
                "invalid_priority" => BadRequest(new ApiError("invalid_priority", "Prioridad inválida.")),
                "invalid_category" => BadRequest(new ApiError("invalid_category", "Categoría inválida.")),
                _ => BadRequest(new ApiError("invalid_request", "Solicitud inválida."))
            };
        }

        return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, new TicketDto(
            ticket.Id, ticket.Subject, ticket.Description, ticket.CategoryId, null,
            ticket.RequesterUserId, ticket.RequesterName, ticket.AssigneeUserId, ticket.Status, ticket.Priority, ticket.Source,
            ticket.Escalated, ticket.CreatedAt, ticket.UpdatedAt));
    }

    [HttpPost("tickets/{id:guid}/escalate")]
    public async Task<IActionResult> Escalate(Guid id)
    {
        var user = RequireUser();
        if (user is null) return Unauthorized(new ApiError("unauthorized", "Autenticación requerida."));

        var (ok, error) = await _manager.EscalateAsync(user, id);
        if (!ok)
        {
            return error switch
            {
                "forbidden" => StatusCode(StatusCodes.Status403Forbidden,
                    new ApiError("forbidden", "Solo Recursos Humanos puede escalar solicitudes.")),
                "not_found" => NotFound(new ApiError("not_found", "Ticket no encontrado.")),
                _ => BadRequest(new ApiError("invalid_request", "No fue posible escalar."))
            };
        }

        return NoContent();
    }

    [HttpGet("escalated")]
    public async Task<IActionResult> Escalated()
    {
        var user = RequireUser();
        if (user is null) return Unauthorized(new ApiError("unauthorized", "Autenticación requerida."));

        if (!user.IsRrhh && !user.IsAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new ApiError("forbidden", "No tiene permisos para ver solicitudes escaladas."));
        }

        return Ok(await _manager.ListEscalatedAsync());
    }

    [HttpDelete("tickets/{id:guid}")]
    public async Task<IActionResult> DeleteTicket(Guid id)
    {
        var user = RequireUser();
        if (user is null) return Unauthorized(new ApiError("unauthorized", "Autenticación requerida."));

        var (ok, error) = await _manager.DeleteAsync(user, id);
        if (!ok)
        {
            return error switch
            {
                "forbidden" => StatusCode(StatusCodes.Status403Forbidden,
                    new ApiError("forbidden", "Solo Recursos Humanos o el administrador pueden eliminar solicitudes.")),
                "not_resolved" => BadRequest(new ApiError("not_resolved", "Solo se pueden eliminar solicitudes resueltas o cerradas.")),
                _ => NotFound(new ApiError("not_found", "Ticket no encontrado."))
            };
        }

        return NoContent();
    }

    [HttpPost("tickets/{id:guid}/comments")]
    public async Task<IActionResult> AddComment(Guid id, [FromBody] AddCommentRequest request)
    {
        var user = RequireUser();
        if (user is null) return Unauthorized(new ApiError("unauthorized", "Autenticación requerida."));

        var added = await _manager.AddCommentAsync(user, id, request.Body);
        return added
            ? NoContent()
            : BadRequest(new ApiError("invalid_comment", "No fue posible agregar el comentario."));
    }

    [HttpPatch("tickets/{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        var user = RequireUser();
        if (user is null) return Unauthorized(new ApiError("unauthorized", "Autenticación requerida."));

        var (ok, error) = await _manager.UpdateStatusAsync(user, id, request.Status, request.AssigneeUserId);
        if (!ok)
        {
            return error switch
            {
                "forbidden" => StatusCode(StatusCodes.Status403Forbidden,
                    new ApiError("forbidden", "No tiene permisos para esta operación.")),
                "not_found" => NotFound(new ApiError("not_found", "Ticket no encontrado.")),
                _ => BadRequest(new ApiError("invalid_status", "Estado inválido."))
            };
        }

        return NoContent();
    }

    private CurrentUser? RequireUser() => _currentUser.User;
}
