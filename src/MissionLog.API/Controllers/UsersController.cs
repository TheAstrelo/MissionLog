using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MissionLog.Core.DTOs;
using MissionLog.Core.Interfaces;

namespace MissionLog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public UsersController(IUnitOfWork uow) => _uow = uow;

    /// <summary>Returns all active users — used to populate assignee dropdowns.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var users = await _uow.Users.GetAllAsync();
        return Ok(users.Select(u => new UserDto(u.Id, u.Username, u.Role)));
    }
}
