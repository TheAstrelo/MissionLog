using Microsoft.AspNetCore.Mvc;
using MissionLog.Core.DTOs;
using MissionLog.Core.Entities;
using MissionLog.Core.Interfaces;
using MissionLog.Infrastructure.Services;

namespace MissionLog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly TokenService _tokenService;

    public AuthController(IUnitOfWork uow, TokenService tokenService)
    {
        _uow = uow;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var user = await _uow.Users.GetByUsernameAsync(dto.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid username or password" });

        if (!user.IsActive)
            return Unauthorized(new { message = "Account is disabled" });

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponseDto(token, user.Username, user.Role, user.Id));
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        var existing = await _uow.Users.GetByUsernameAsync(dto.Username);
        if (existing != null)
            return BadRequest(new { message = "Username already taken" });

        var validRoles = new[] { "Technician", "Engineer", "Supervisor", "Admin" };
        if (!validRoles.Contains(dto.Role))
            return BadRequest(new { message = "Invalid role" });

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role
        };

        await _uow.Users.CreateAsync(user);

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponseDto(token, user.Username, user.Role, user.Id));
    }
}
