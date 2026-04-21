using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FortunaCasino.Data;
using FortunaCasino.DTOs;
using FortunaCasino.DTOs.Auth;
using FortunaCasino.Models;
using FortunaCasino.Services.Interfaces;
using BCrypt.Net;

namespace FortunaCasino.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly IEmailService _emailService;

    public AuthService(AppDbContext context, IConfiguration config, IEmailService emailService)
    {
        _context = context;
        _config = config;
        _emailService = emailService;
    }

    //Regisztráció
    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return null;

        var confirmationToken = Guid.NewGuid().ToString("N");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Balance = 10000.00m,
            EmailConfirmed = false,
            EmailConfirmationToken = confirmationToken
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = 1 });
        await _context.SaveChangesAsync();

        // Email küldés (ha nem megy, nem baj)
        try
        {
            var confirmationLink = $"http://localhost:5173/confirm-email?token={confirmationToken}&userId={user.Id}";

            var emailBody = $@"
            <h2>Kedves {request.Username}!</h2>
            <p>Köszönjük a regisztrációt!</p>
            <a href='{confirmationLink}' style='padding:10px 20px;background:#007bff;color:white;text-decoration:none;border-radius:5px;'>
               Email megerősítése
            </a>
        ";

            await _emailService.SendEmailAsync(request.Email,
                "FortunaCasino - Email megerősítés", emailBody);
        }
        catch
        {
        }

        return new AuthResponse
        {
            Token = null!,
            User = MapToDto(user)
        };
    }

    //Bejelentkezés
    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        if (!user.IsActive)
        {
            Console.WriteLine($"[BAN] {user.Username} kitiltott felhasználó próbált belépni!");
            return new AuthResponse { IsBanned = true };
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return new AuthResponse
        {
            Token = GenerateJwtToken(user),
            User = MapToDto(user)
        };
    }

    //Emailes megerősítés
    public async Task<bool> ConfirmEmailAsync(long userId, string token)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.EmailConfirmationToken != token)
            return false;

        user.EmailConfirmed = true;
        user.EmailConfirmationToken = null;
        await _context.SaveChangesAsync();
        return true;
    }

    //Email újraküldés
    public async Task<bool> ResendConfirmationAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || user.EmailConfirmed)
            return false;

        var confirmationToken = Guid.NewGuid().ToString("N");
        user.EmailConfirmationToken = confirmationToken;
        await _context.SaveChangesAsync();

        var frontendUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:5173";
        var confirmationLink = $"{frontendUrl}/confirm-email?token={confirmationToken}&userId={user.Id}";

        var emailBody = $@"
            <h2>Kedves {user.Username}!</h2>
            <p>Új megerősítő link:</p>
            <a href='{confirmationLink}' 
               style='padding:10px 20px;background:#007bff;color:white;text-decoration:none;border-radius:5px;'>
               Email megerősítése
            </a>
        ";

        await _emailService.SendEmailAsync(user.Email,
            "FortunaCasino - Email megerősítés (újra)", emailBody);

        return true;
    }

    //User id lekérdezés
    public async Task<User?> GetUserById(long id)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    //Jwt token generálás
    public string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email)
        };

        claims.AddRange(user.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    //DTO konvertálás
    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Balance = user.Balance,
        Roles = user.Roles,
        EmailConfirmed = user.EmailConfirmed
    };

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return true;

        var resetToken = Guid.NewGuid().ToString("N");
        user.PasswordResetToken = resetToken;
        user.PasswordResetExpires = DateTime.UtcNow.AddHours(1);
        await _context.SaveChangesAsync();

        var frontendUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:5173";
        var resetLink = $"{frontendUrl}/reset-password?token={resetToken}&userId={user.Id}";

        var emailBody = $@"
            <h2>Jelszó visszaállítás</h2>
            <p>Kedves <strong>{user.Username}</strong>!</p>
            <a href='{resetLink}' style='padding:10px 20px;background:#007bff;color:white;text-decoration:none;border-radius:5px;'>
                Jelszó visszaállítása
            </a>
            <p style='color:#888;font-size:13px;'>Ez a link 1 óráig érvényes.</p>";

        await _emailService.SendEmailAsync(user.Email, "FortunaCasino - Jelszó visszaállítás", emailBody);
        return true;
    }

    //Jelszó reset
    public async Task<bool> ResetPasswordAsync(string token, long userId, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null
            || user.PasswordResetToken != token
            || user.PasswordResetExpires == null
            || user.PasswordResetExpires < DateTime.UtcNow)
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetExpires = null;
        await _context.SaveChangesAsync();
        return true;
    }

    //Összes user lekérdezése
    public async Task<object> GetAll()
    {
        return await _context.Users.ToListAsync();
    }
}