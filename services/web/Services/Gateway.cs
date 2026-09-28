using System.Text.Json;
using PluriOne.Web.Contracts;

namespace PluriOne.Web.Services;

/// <summary>
/// Fachada tipada para consumir los microservicios de Onboarding y HR.
/// </summary>
public sealed class Gateway
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<Gateway> _logger;

    public Gateway(IHttpClientFactory httpClientFactory, ILogger<Gateway> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    // --- Onboarding ---

    public async Task<EmployeeDto?> GetEmployeeAsync()
    {
        var response = await _httpClientFactory.CreateClient("onboarding").GetAsync("/api/onboarding/me");
        return await ReadAsync<EmployeeDto>(response);
    }

    public async Task<List<TaskItemDto>> GetTasksAsync()
    {
        var response = await _httpClientFactory.CreateClient("onboarding").GetAsync("/api/onboarding/tasks");
        return await ReadAsync<List<TaskItemDto>>(response) ?? new List<TaskItemDto>();
    }

    public async Task<ProgressDto?> GetProgressAsync()
    {
        var response = await _httpClientFactory.CreateClient("onboarding").GetAsync("/api/onboarding/progress");
        return await ReadAsync<ProgressDto>(response);
    }

    public async Task<List<PlanPhaseDto>> GetPlanAsync()
    {
        var response = await _httpClientFactory.CreateClient("onboarding").GetAsync("/api/onboarding/plan");
        return await ReadAsync<List<PlanPhaseDto>>(response) ?? new List<PlanPhaseDto>();
    }

    public async Task<bool> UpdateTaskStatusAsync(Guid taskId, string status)
    {
        var client = _httpClientFactory.CreateClient("onboarding");
        var response = await client.PutAsJsonAsync($"/api/onboarding/tasks/{taskId}/status", new { status });
        return response.IsSuccessStatusCode;
    }

    // --- HR ---

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        var response = await _httpClientFactory.CreateClient("hr").GetAsync("/api/hr/categories");
        return await ReadAsync<List<CategoryDto>>(response) ?? new List<CategoryDto>();
    }

    public async Task<List<TicketDto>> GetTicketsAsync()
    {
        var response = await _httpClientFactory.CreateClient("hr").GetAsync("/api/hr/tickets");
        return await ReadAsync<List<TicketDto>>(response) ?? new List<TicketDto>();
    }

    public async Task<TicketDetailDto?> GetTicketAsync(Guid id)
    {
        var response = await _httpClientFactory.CreateClient("hr").GetAsync($"/api/hr/tickets/{id}");
        return await ReadAsync<TicketDetailDto>(response);
    }

    public async Task<(TicketDto? Ticket, string? Error)> CreateTicketAsync(string subject, string? description, Guid? categoryId, string priority)
    {
        var client = _httpClientFactory.CreateClient("hr");
        var response = await client.PostAsJsonAsync("/api/hr/tickets", new
        {
            subject,
            description,
            categoryId,
            priority,
            source = "manual"
        });

        if (response.IsSuccessStatusCode)
        {
            return (await ReadAsync<TicketDto>(response), null);
        }

        return (null, await ReadErrorAsync(response));
    }

    public async Task<bool> AddCommentAsync(Guid ticketId, string body)
    {
        var client = _httpClientFactory.CreateClient("hr");
        var response = await client.PostAsJsonAsync($"/api/hr/tickets/{ticketId}/comments", new { body });
        return response.IsSuccessStatusCode;
    }

    public async Task<(bool Ok, string? Error)> UpdateTicketStatusAsync(Guid ticketId, string status, Guid? assigneeUserId)
    {
        var client = _httpClientFactory.CreateClient("hr");
        var response = await client.PatchAsJsonAsync($"/api/hr/tickets/{ticketId}/status", new { status, assigneeUserId });
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        return (false, await ReadErrorAsync(response));
    }

    public async Task<(bool Ok, string? Error)> EscalateTicketAsync(Guid ticketId)
    {
        var client = _httpClientFactory.CreateClient("hr");
        var response = await client.PostAsync($"/api/hr/tickets/{ticketId}/escalate", content: null);
        return response.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(response));
    }

    public async Task<(bool Ok, string? Error)> DeleteTicketAsync(Guid ticketId)
    {
        var client = _httpClientFactory.CreateClient("hr");
        var response = await client.DeleteAsync($"/api/hr/tickets/{ticketId}");
        return response.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(response));
    }

    public async Task<List<TicketDto>> GetEscalatedTicketsAsync()
    {
        var response = await _httpClientFactory.CreateClient("hr").GetAsync("/api/hr/escalated");
        return await ReadAsync<List<TicketDto>>(response) ?? new List<TicketDto>();
    }

    // --- Perfil ---

    public async Task<(bool Ok, string? Error)> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var client = _httpClientFactory.CreateClient("auth");
        var response = await client.PostAsJsonAsync("/api/auth/change-password", new { currentPassword, newPassword });
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        return (false, await ReadErrorAsync(response));
    }

    public async Task<bool> UpdateFullNameAsync(string fullName)
    {
        var client = _httpClientFactory.CreateClient("auth");
        var response = await client.PatchAsJsonAsync("/api/auth/profile", new { fullName });
        return response.IsSuccessStatusCode;
    }

    // --- Admin ---

    public async Task<List<AdminUserDto>> GetUsersAsync()
    {
        var response = await _httpClientFactory.CreateClient("auth").GetAsync("/api/admin/users");
        return await ReadAsync<List<AdminUserDto>>(response) ?? new List<AdminUserDto>();
    }

    public async Task<(bool Ok, string? Error)> CreateUserAsync(string email, string fullName, string role, string password, string? employeeCode)
    {
        var client = _httpClientFactory.CreateClient("auth");
        var response = await client.PostAsJsonAsync("/api/admin/users", new { email, fullName, role, password, employeeCode });
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        return (false, await ReadErrorAsync(response));
    }

    public async Task<(bool Ok, string? Error)> UpdateUserRoleAsync(Guid userId, string role)
    {
        var client = _httpClientFactory.CreateClient("auth");
        var response = await client.PatchAsJsonAsync($"/api/admin/users/{userId}/role", new { role });
        return response.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(response));
    }

    public async Task<(bool Ok, string? Error)> UpdateUserStatusAsync(Guid userId, bool isActive)
    {
        var client = _httpClientFactory.CreateClient("auth");
        var response = await client.PatchAsJsonAsync($"/api/admin/users/{userId}/status", new { isActive });
        return response.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(response));
    }

    public async Task<List<AuditEventDto>> GetAuditAsync()
    {
        var response = await _httpClientFactory.CreateClient("auth").GetAsync("/api/admin/audit");
        return await ReadAsync<List<AuditEventDto>>(response) ?? new List<AuditEventDto>();
    }

    public async Task<List<AuditEventDto>> GetAuditByDateAsync(DateTime date)
    {
        var response = await _httpClientFactory.CreateClient("auth")
            .GetAsync($"/api/admin/audit/by-date?date={date:yyyy-MM-dd}");
        return await ReadAsync<List<AuditEventDto>>(response) ?? new List<AuditEventDto>();
    }

    public async Task<List<AuditDaySummary>> GetAuditSummaryAsync(int year, int month)
    {
        var response = await _httpClientFactory.CreateClient("auth")
            .GetAsync($"/api/admin/audit/summary?year={year}&month={month}");
        return await ReadAsync<List<AuditDaySummary>>(response) ?? new List<AuditDaySummary>();
    }

    public async Task<(bool Ok, string? Error)> ResetUserPasswordAsync(Guid userId, string newPassword)
    {
        var client = _httpClientFactory.CreateClient("auth");
        var response = await client.PostAsJsonAsync($"/api/admin/users/{userId}/reset-password", new { newPassword });
        return response.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(response));
    }

    public async Task<(bool Ok, string? Result)> IngestKnowledgeBaseAsync()
    {
        var response = await _httpClientFactory.CreateClient("ai").PostAsync("/ingest", content: null);
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return (true, json);
        }

        return (false, await ReadErrorAsync(response));
    }

    public async Task<List<DocumentDto>> GetDocumentsAsync()
    {
        var response = await _httpClientFactory.CreateClient("ai").GetAsync("/documents");
        return await ReadAsync<List<DocumentDto>>(response) ?? new List<DocumentDto>();
    }

    public async Task<(bool Ok, string? Error)> DeleteDocumentAsync(string documentId)
    {
        var client = _httpClientFactory.CreateClient("ai");
        var response = await client.DeleteAsync($"/documents/{documentId}");
        return response.IsSuccessStatusCode ? (true, null) : (false, await ReadErrorAsync(response));
    }

    // --- helpers ---

    private async Task<T?> ReadAsync<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Gateway: {Method} {Uri} -> {Status}",
                response.RequestMessage?.Method, response.RequestMessage?.RequestUri, (int)response.StatusCode);
            return default;
        }

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var json = await response.Content.ReadAsStringAsync();
            var error = JsonSerializer.Deserialize<PluriOne.Shared.Contracts.ApiError>(json, JsonOptions);
            return error?.Code ?? "error";
        }
        catch
        {
            return "error";
        }
    }
}
