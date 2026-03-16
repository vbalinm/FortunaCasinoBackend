using FortunaCasino.DTOs.Auth;
using FortunaCasino.Models;

namespace FortunaCasino.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<bool> ConfirmEmailAsync(long userId, string token);
    Task<bool> ResendConfirmationAsync(string email);
    Task<User?> GetUserById(long id);  // EZ KELL!
    string GenerateJwtToken(User user);
    Task<object> GetAll();
}