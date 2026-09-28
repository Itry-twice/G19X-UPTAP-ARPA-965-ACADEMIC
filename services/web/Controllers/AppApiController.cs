using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PluriOne.Shared.Auth;
using PluriOne.Web.Services;

namespace PluriOne.Web.Controllers;

[Authorize]
[ApiController]
[Route("api")]
public sealed class AppApiController : ControllerBase
{
    private readonly Gateway _gateway;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICurrentUserContext _currentUser;

    public AppApiController(Gateway gateway, IHttpClientFactory httpClientFactory, ICurrentUserContext currentUser)
    {
        _gateway = gateway;
        _httpClientFactory = httpClientFactory;
        _currentUser = currentUser;
    }

    public sealed record UpdateTaskStatusRequest(string Status);
    public sealed record ChatRequest(string Question);

    [HttpPost("onboarding/tasks/{taskId:guid}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateTaskStatus(Guid taskId, [FromBody] UpdateTaskStatusRequest request)
    {
        var ok = await _gateway.UpdateTaskStatusAsync(taskId, request.Status);
        return ok ? NoContent() : BadRequest(new { error = "invalid_status" });
    }

    [HttpPost("admin/upload")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDocument([FromForm] IFormFile file)
    {
        if (_currentUser.User?.IsAdmin != true)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "forbidden" });
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "empty_file" });
        }

        var client = _httpClientFactory.CreateClient("ai");
        using var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
        content.Add(fileContent, "file", file.FileName);

        using var upstream = new HttpRequestMessage(HttpMethod.Post, "/upload") { Content = content };
        using var response = await client.SendAsync(upstream);

        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, body);
        }

        return Ok(body);
    }

    [HttpPost("assistant/stream")]
    [ValidateAntiForgeryToken]
    public async Task StreamAssistant([FromBody] ChatRequest request)
    {
        var client = _httpClientFactory.CreateClient("ai");
        using var upstream = new HttpRequestMessage(HttpMethod.Post, "/chat/stream")
        {
            Content = JsonContent.Create(new { question = request.Question })
        };

        using var response = await client.SendAsync(upstream, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
        {
            Response.StatusCode = (int)response.StatusCode;
            Response.ContentType = "application/json";
            await response.Content.CopyToAsync(Response.Body);
            return;
        }

        Response.StatusCode = 200;
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers["X-Accel-Buffering"] = "no";

        await using var stream = await response.Content.ReadAsStreamAsync();
        var buffer = new byte[8192];
        int read;
        while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
        {
            await Response.Body.WriteAsync(buffer.AsMemory(0, read));
            await Response.Body.FlushAsync();
        }
    }
}
