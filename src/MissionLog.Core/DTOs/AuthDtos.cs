namespace MissionLog.Core.DTOs;

public record LoginDto(string Username, string Password);

public record RegisterDto(string Username, string Email, string Password, string Role);

public record AuthResponseDto(string Token, string Username, string Role, int UserId);
