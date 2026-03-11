using MissionLog.Core.DTOs;

namespace MissionLog.BlazorApp.Services;

public class AuthStateService
{
    public AuthResponseDto? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;
    public string? Token => CurrentUser?.Token;

    public event Action? OnChange;

    public void SetUser(AuthResponseDto user)
    {
        CurrentUser = user;
        OnChange?.Invoke();
    }

    public void Logout()
    {
        CurrentUser = null;
        OnChange?.Invoke();
    }
}
