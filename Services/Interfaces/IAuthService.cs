using FortunaCasino.DTOs.Auth;
using FortunaCasino.Models;

namespace FortunaCasino.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    string GenerateJwtToken(User user);
    Task<object> GetAll();
}