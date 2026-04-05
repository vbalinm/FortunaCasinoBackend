using FortunaCasino.DTOs.Auth;
using FortunaCasino.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FortunaCasino.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IAuthService authService, ICurrentUserService currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return result == null
            ? BadRequest("Foglalt felhasználónév")
            : Ok(new { Message = "Regisztráció sikeres! Kérlek erősítsd meg az email címedet.", User = result.User });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null)
            return Unauthorized("Hibás adatok vagy nem erősítetted meg az email címedet");
        return Ok(result);
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(long userId, string token)
    {
        var result = await _authService.ConfirmEmailAsync(userId, token);
        return result
            ? Ok("Email sikeresen megerősítve!")
            : BadRequest("Érvénytelen token");
    }

    [Authorize]
    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation()
    {
        var userId = _currentUser.GetUserId();
        var user = await _authService.GetUserById(userId);

        if (user == null || user.EmailConfirmed)
            return BadRequest("Már megerősített email vagy nem található");

        var result = await _authService.ResendConfirmationAsync(user.Email);
        return result
            ? Ok("Megerősítő email elküldve!")
            : BadRequest("Hiba történt");
    }

    [Authorize]
    [HttpGet("email-status")]
    public async Task<IActionResult> GetEmailStatus()
    {
        var userId = _currentUser.GetUserId();
        var user = await _authService.GetUserById(userId);

        if (user == null) return NotFound();

        return Ok(new { Email = user.Email, IsConfirmed = user.EmailConfirmed });
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        return Ok(await _authService.GetAll());
    }
}