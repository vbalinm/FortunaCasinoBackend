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

        // ✅ Szép email megerősítő sablon
        try
        {
            var confirmationLink = $"http://localhost:5173/confirm-email?token={confirmationToken}&userId={user.Id}";

            var emailBody = $@"
                <div style='font-family:Segoe UI,sans-serif;max-width:600px;margin:0 auto;'>
                    <div style='background:linear-gradient(135deg,#f59e0b,#ea580c);padding:30px;border-radius:12px 12px 0 0;text-align:center;'>
                        <h1 style='color:white;margin:0;font-size:28px;'>🎰 Fortuna Lotto</h1>
                        <p style='color:rgba(255,255,255,0.9);margin:8px 0 0;font-size:16px;'>Üdvözlünk a családban!</p>
                    </div>
                    <div style='background:#fff;padding:30px;border-radius:0 0 12px 12px;box-shadow:0 4px 20px rgba(0,0,0,0.1);'>
                        <h2 style='color:#1f2937;margin-top:0;'>Kedves {request.Username}! 👋</h2>
                        <p style='color:#4b5563;line-height:1.6;'>
                            Köszönjük a regisztrációt! Már csak egy lépés választ el attól, 
                            hogy elkezdd a játékot. Erősítsd meg az email címedet az alábbi gombra kattintva.
                        </p>

                        <div style='text-align:center;margin:30px 0;'>
                            <a href='{confirmationLink}' 
                               style='background:linear-gradient(135deg,#f59e0b,#ea580c);
                                      color:white;
                                      text-decoration:none;
                                      padding:14px 32px;
                                      border-radius:8px;
                                      font-weight:bold;
                                      font-size:16px;
                                      display:inline-block;'>
                                ✅ Email megerősítése
                            </a>
                        </div>

                        <div style='background:#fef3c7;border:1px solid #fde68a;border-radius:8px;padding:15px;margin:20px 0;'>
                            <p style='margin:0;color:#92400e;font-size:14px;'>
                                🎁 <strong>Bónusz:</strong> Sikeres megerősítés után 10 000 Ft egyenleg vár rád!
                            </p>
                        </div>

                        <p style='color:#9ca3af;font-size:13px;margin-top:20px;'>
                            Ha nem te regisztráltál, hagyd figyelmen kívül ezt az emailt.<br/>
                            A link 24 óráig érvényes.
                        </p>
                        <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0;'/>
                        <p style='color:#9ca3af;font-size:12px;text-align:center;margin:0;'>
                            © {DateTime.Now.Year} Fortuna Lotto · Minden jog fenntartva
                        </p>
                    </div>
                </div>";

            await _emailService.SendEmailAsync(request.Email,
                "🎰 Fortuna Lotto - Erősítsd meg az email címedet!", emailBody);
        }
        catch { }

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

        user.LastLoginAt = DateTime.Now;
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

        // ✅ Szép újraküldés sablon
        var emailBody = $@"
            <div style='font-family:Segoe UI,sans-serif;max-width:600px;margin:0 auto;'>
                <div style='background:linear-gradient(135deg,#f59e0b,#ea580c);padding:30px;border-radius:12px 12px 0 0;text-align:center;'>
                    <h1 style='color:white;margin:0;font-size:28px;'>🎰 Fortuna Lotto</h1>
                    <p style='color:rgba(255,255,255,0.9);margin:8px 0 0;font-size:16px;'>Email megerősítés</p>
                </div>
                <div style='background:#fff;padding:30px;border-radius:0 0 12px 12px;box-shadow:0 4px 20px rgba(0,0,0,0.1);'>
                    <h2 style='color:#1f2937;margin-top:0;'>Kedves {user.Username}! 👋</h2>
                    <p style='color:#4b5563;line-height:1.6;'>
                        Új megerősítő linket kértél. Kattints az alábbi gombra az email cím megerősítéséhez.
                    </p>

                    <div style='text-align:center;margin:30px 0;'>
                        <a href='{confirmationLink}' 
                           style='background:linear-gradient(135deg,#f59e0b,#ea580c);
                                  color:white;
                                  text-decoration:none;
                                  padding:14px 32px;
                                  border-radius:8px;
                                  font-weight:bold;
                                  font-size:16px;
                                  display:inline-block;'>
                            ✅ Email megerősítése
                        </a>
                    </div>

                    <p style='color:#9ca3af;font-size:13px;margin-top:20px;'>
                        Ha nem te kérted, hagyd figyelmen kívül ezt az emailt.<br/>
                        A link 24 óráig érvényes.
                    </p>
                    <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0;'/>
                    <p style='color:#9ca3af;font-size:12px;text-align:center;margin:0;'>
                        © {DateTime.Now.Year} Fortuna Lotto · Minden jog fenntartva
                    </p>
                </div>
            </div>";

        await _emailService.SendEmailAsync(user.Email,
            "🎰 Fortuna Lotto - Email megerősítés (újra)", emailBody);

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
            expires: DateTime.Now.AddHours(24),
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
        user.PasswordResetExpires = DateTime.Now.AddHours(1);
        await _context.SaveChangesAsync();

        var frontendUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:5173";
        var resetLink = $"{frontendUrl}/reset-password?token={resetToken}&userId={user.Id}";

        // ✅ Szép jelszó visszaállítás sablon
        var emailBody = $@"
            <div style='font-family:Segoe UI,sans-serif;max-width:600px;margin:0 auto;'>
                <div style='background:linear-gradient(135deg,#dc2626,#b91c1c);padding:30px;border-radius:12px 12px 0 0;text-align:center;'>
                    <h1 style='color:white;margin:0;font-size:28px;'>🔒 Fortuna Lotto</h1>
                    <p style='color:rgba(255,255,255,0.9);margin:8px 0 0;font-size:16px;'>Jelszó visszaállítás</p>
                </div>
                <div style='background:#fff;padding:30px;border-radius:0 0 12px 12px;box-shadow:0 4px 20px rgba(0,0,0,0.1);'>
                    <h2 style='color:#1f2937;margin-top:0;'>Kedves {user.Username}! 👋</h2>
                    <p style='color:#4b5563;line-height:1.6;'>
                        Jelszó visszaállítást kértél a fiókodhoz. 
                        Kattints az alábbi gombra az új jelszó beállításához.
                    </p>

                    <div style='text-align:center;margin:30px 0;'>
                        <a href='{resetLink}' 
                           style='background:linear-gradient(135deg,#dc2626,#b91c1c);
                                  color:white;
                                  text-decoration:none;
                                  padding:14px 32px;
                                  border-radius:8px;
                                  font-weight:bold;
                                  font-size:16px;
                                  display:inline-block;'>
                            🔑 Jelszó visszaállítása
                        </a>
                    </div>

                    <div style='background:#fef2f2;border:1px solid #fecaca;border-radius:8px;padding:15px;margin:20px 0;'>
                        <p style='margin:0;color:#991b1b;font-size:14px;'>
                            ⚠️ <strong>Figyelem:</strong> Ez a link <strong>1 óráig</strong> érvényes.<br/>
                            Ha nem te kérted a visszaállítást, azonnal változtasd meg a jelszavadat!
                        </p>
                    </div>

                    <p style='color:#9ca3af;font-size:13px;margin-top:20px;'>
                        Ha nem tudod megnyitni a gombot, másold be ezt a linket a böngésződbe:<br/>
                        <a href='{resetLink}' style='color:#f59e0b;word-break:break-all;'>{resetLink}</a>
                    </p>
                    <hr style='border:none;border-top:1px solid #e5e7eb;margin:20px 0;'/>
                    <p style='color:#9ca3af;font-size:12px;text-align:center;margin:0;'>
                        © {DateTime.Now.Year} Fortuna Lotto · Minden jog fenntartva
                    </p>
                </div>
            </div>";

        await _emailService.SendEmailAsync(user.Email, "🔒 Fortuna Lotto - Jelszó visszaállítás", emailBody);
        return true;
    }

    //Jelszó reset
    public async Task<bool> ResetPasswordAsync(string token, long userId, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null
            || user.PasswordResetToken != token
            || user.PasswordResetExpires == null
            || user.PasswordResetExpires < DateTime.Now)
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