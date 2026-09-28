using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PluriOne.OnboardingService.Dtos;
using PluriOne.OnboardingService.Services;
using PluriOne.Shared.Auth;
using PluriOne.Shared.Contracts;

namespace PluriOne.OnboardingService.Controllers;

[ApiController]
[Authorize]
[Route("api/onboarding")]
public sealed class OnboardingController : ControllerBase
{
    private readonly OnboardingManager _manager;
    private readonly ICurrentUserContext _currentUser;

    public OnboardingController(OnboardingManager manager, ICurrentUserContext currentUser)
    {
        _manager = manager;
        _currentUser = currentUser;
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = RequireColaborador();
        if (user is null) return Forbidden();

        var employee = await _manager.GetOrCreateEmployeeAsync(user);
        return Ok(new EmployeeDto(employee.Id, employee.FullName, employee.Email, employee.Position, employee.StartDate));
    }

    [HttpGet("tasks")]
    public async Task<IActionResult> Tasks()
    {
        var user = RequireColaborador();
        if (user is null) return Forbidden();

        var employee = await _manager.GetOrCreateEmployeeAsync(user);
        var tasks = await _manager.GetTasksAsync(employee.Id);
        return Ok(tasks);
    }

    [HttpPut("tasks/{taskId:guid}/status")]
    public async Task<IActionResult> UpdateTaskStatus(Guid taskId, [FromBody] UpdateTaskStatusRequest request)
    {
        var user = RequireColaborador();
        if (user is null) return Forbidden();

        var employee = await _manager.GetOrCreateEmployeeAsync(user);
        var updated = await _manager.UpdateTaskStatusAsync(employee.Id, taskId, request.Status);

        return updated
            ? NoContent()
            : BadRequest(new ApiError("invalid_status", "Estado inválido o tarea no encontrada."));
    }

    [HttpGet("progress")]
    public async Task<IActionResult> Progress()
    {
        var user = RequireColaborador();
        if (user is null) return Forbidden();

        var employee = await _manager.GetOrCreateEmployeeAsync(user);
        var progress = await _manager.GetProgressAsync(employee.Id);
        return Ok(progress);
    }

    [HttpGet("plan")]
    public async Task<IActionResult> Plan()
    {
        var plan = await _manager.GetPlanAsync();
        return Ok(plan);
    }

    private CurrentUser? RequireColaborador()
    {
        var user = _currentUser.User;
        return user?.IsColaborador == true ? user : null;
    }

    private IActionResult Forbidden()
    {
        return StatusCode(StatusCodes.Status403Forbidden,
            new ApiError("forbidden", "El onboarding personal es exclusivo del rol colaborador."));
    }
}
