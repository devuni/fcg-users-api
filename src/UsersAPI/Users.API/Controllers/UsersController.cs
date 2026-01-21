using Contracts.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Users.Application.DTOs;
using Users.Application.Interfaces;

namespace Users.API.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly IEventBus _bus;

    public UsersController(IUserRepository repo, IPasswordHasher hasher, IEventBus bus)
    {
        _repo = repo;
        _hasher = hasher;
        _bus = bus;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Name, Email e Password são obrigatórios.");

        var exists = await _repo.ExistsByEmailAsync(request.Email, ct);
        if (exists) return Conflict("Email já cadastrado.");

        var passwordHash = _hasher.Hash(request.Password);
        var userId = await _repo.CreateAsync(request.Name, request.Email, passwordHash, ct);

        await _bus.PublishAsync(new UserCreatedEvent(userId, request.Name, request.Email), ct);

        return Created($"/api/v1/users/{userId}", new { id = userId });
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id) => Ok(new { id }); // demo protegido

    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "ok" });
}
