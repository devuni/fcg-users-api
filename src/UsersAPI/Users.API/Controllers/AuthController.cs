using Microsoft.AspNetCore.Mvc;
using Users.Application.DTOs;
using Users.Application.Interfaces;

namespace Users.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokenService;

    public AuthController(IUserRepository repo, IPasswordHasher hasher, ITokenService tokenService)
    {
        _repo = repo;
        _hasher = hasher;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email e Password são obrigatórios.");

        var user = await _repo.GetByEmailAsync(request.Email, ct);
        if (user is null)
            return Unauthorized("Credenciais inválidas.");

        var ok = _hasher.Verify(request.Password, user.Value.PasswordHash);
        if (!ok)
            return Unauthorized("Credenciais inválidas.");

        var token = _tokenService.CreateToken(user.Value.Id, user.Value.Name, user.Value.Email);

        return Ok(new LoginResponse(token, DateTime.UtcNow.AddHours(1)));
    }
}
