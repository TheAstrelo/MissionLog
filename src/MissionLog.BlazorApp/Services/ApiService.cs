using System.Net.Http.Json;
using MissionLog.Core.DTOs;
using MissionLog.Core.Enums;

namespace MissionLog.BlazorApp.Services;

public class ApiService
{
    private readonly HttpClient _http;

    public ApiService(HttpClient http)
    {
        _http = http;
    }

    // Auth
    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto) =>
        await _http.PostAsJsonAsync("api/auth/login", dto)
            .ContinueWith(async t => await (await t).Content.ReadFromJsonAsync<AuthResponseDto>())
            .Unwrap();

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto) =>
        await _http.PostAsJsonAsync("api/auth/register", dto)
            .ContinueWith(async t => await (await t).Content.ReadFromJsonAsync<AuthResponseDto>())
            .Unwrap();

    // Work Orders
    public async Task<List<WorkOrderDto>?> GetWorkOrdersAsync(WorkOrderStatus? status = null)
    {
        var url = status.HasValue ? $"api/workorders?status={(int)status}" : "api/workorders";
        return await _http.GetFromJsonAsync<List<WorkOrderDto>>(url);
    }

    public async Task<List<WorkOrderDto>?> GetMyWorkOrdersAsync() =>
        await _http.GetFromJsonAsync<List<WorkOrderDto>>("api/workorders/my");

    public async Task<WorkOrderSummaryDto?> GetSummaryAsync() =>
        await _http.GetFromJsonAsync<WorkOrderSummaryDto>("api/workorders/summary");

    public async Task<WorkOrderDto?> GetWorkOrderAsync(int id) =>
        await _http.GetFromJsonAsync<WorkOrderDto>($"api/workorders/{id}");

    public async Task<WorkOrderDto?> CreateWorkOrderAsync(CreateWorkOrderDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/workorders", dto);
        return await response.Content.ReadFromJsonAsync<WorkOrderDto>();
    }

    public async Task<WorkOrderDto?> ApproveAsync(int id, string notes)
    {
        var response = await _http.PostAsJsonAsync($"api/workorders/{id}/approve", new ApprovalActionDto("Approved", notes));
        return await response.Content.ReadFromJsonAsync<WorkOrderDto>();
    }

    public async Task<WorkOrderDto?> RejectAsync(int id, string notes)
    {
        var response = await _http.PostAsJsonAsync($"api/workorders/{id}/reject", new ApprovalActionDto("Rejected", notes));
        return await response.Content.ReadFromJsonAsync<WorkOrderDto>();
    }

    public async Task<WorkOrderDto?> SubmitAsync(int id)
    {
        var response = await _http.PostAsync($"api/workorders/{id}/submit", null);
        return await response.Content.ReadFromJsonAsync<WorkOrderDto>();
    }
}
