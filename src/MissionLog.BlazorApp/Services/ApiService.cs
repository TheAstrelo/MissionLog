using System.Net.Http.Headers;
using System.Net.Http.Json;
using MissionLog.Core.DTOs;
using MissionLog.Core.Enums;

namespace MissionLog.BlazorApp.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private readonly AuthStateService _auth;

    public ApiService(HttpClient http, AuthStateService auth)
    {
        _http = http;
        _auth = auth;
    }

    // Ensure JWT is always attached before every request
    private void AttachToken()
    {
        if (_auth.Token != null)
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _auth.Token);
    }

    // ── Auth ──────────────────────────────────────────────────────────────────
    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", dto);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<AuthResponseDto>()
            : null;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", dto);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<AuthResponseDto>()
            : null;
    }

    // ── Work Orders ───────────────────────────────────────────────────────────
    public async Task<List<WorkOrderDto>?> GetWorkOrdersAsync(WorkOrderStatus? status = null)
    {
        AttachToken();
        var url = status.HasValue ? $"api/workorders?status={(int)status}" : "api/workorders";
        return await _http.GetFromJsonAsync<List<WorkOrderDto>>(url);
    }

    public async Task<List<WorkOrderDto>?> GetMyWorkOrdersAsync()
    {
        AttachToken();
        return await _http.GetFromJsonAsync<List<WorkOrderDto>>("api/workorders/my");
    }

    public async Task<WorkOrderSummaryDto?> GetSummaryAsync()
    {
        AttachToken();
        return await _http.GetFromJsonAsync<WorkOrderSummaryDto>("api/workorders/summary");
    }

    public async Task<WorkOrderDto?> GetWorkOrderAsync(int id)
    {
        AttachToken();
        return await _http.GetFromJsonAsync<WorkOrderDto>($"api/workorders/{id}");
    }

    public async Task<WorkOrderDto?> CreateWorkOrderAsync(CreateWorkOrderDto dto)
    {
        AttachToken();
        var response = await _http.PostAsJsonAsync("api/workorders", dto);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<WorkOrderDto>()
            : null;
    }

    // ── Workflow transitions ──────────────────────────────────────────────────
    public async Task<WorkOrderDto?> SubmitAsync(int id)
        => await PostTransition($"api/workorders/{id}/submit");

    public async Task<WorkOrderDto?> StartAsync(int id)
        => await PostTransition($"api/workorders/{id}/start");

    public async Task<WorkOrderDto?> CompleteAsync(int id)
        => await PostTransition($"api/workorders/{id}/complete");

    public async Task<WorkOrderDto?> ApproveAsync(int id, string? notes)
    {
        AttachToken();
        var response = await _http.PostAsJsonAsync(
            $"api/workorders/{id}/approve",
            new ApprovalActionDto("Approved", notes));
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<WorkOrderDto>()
            : null;
    }

    public async Task<WorkOrderDto?> RejectAsync(int id, string? notes)
    {
        AttachToken();
        var response = await _http.PostAsJsonAsync(
            $"api/workorders/{id}/reject",
            new ApprovalActionDto("Rejected", notes));
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<WorkOrderDto>()
            : null;
    }

    private async Task<WorkOrderDto?> PostTransition(string url)
    {
        AttachToken();
        var response = await _http.PostAsync(url, null);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<WorkOrderDto>()
            : null;
    }
}
