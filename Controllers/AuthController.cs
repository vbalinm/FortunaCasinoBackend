using Microsoft.AspNetCore.Mvc;
using FortunaCasino.DTOs.Auth;
using FortunaCasino.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace FortunaCasino.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return result == null ? BadRequest("Foglalt felhasználónév") : Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return result == null ? Unauthorized("Hibás adatok") : Ok(result);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult> GEtAll()
    {
        return Ok(await _authService.GetAll());
    }
}