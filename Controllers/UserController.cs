using FortunaCasino.Controllers.FortunaCasino.DTOs;
using FortunaCasino.Data;
using FortunaCasino.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FortunaCasino.Controllers
{ 
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public UserController(AppDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = _currentUser.GetUserId();

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.Balance,
                user.EmailConfirmed,
                Roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList()
            });
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = _currentUser.GetUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                var taken = await _context.Users
                    .AnyAsync(u => u.Username == request.Username && u.Id != userId);
                if (taken) return BadRequest("Ez a felhasználónév már foglalt");
                user.Username = request.Username;
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var taken = await _context.Users
                    .AnyAsync(u => u.Email == request.Email && u.Id != userId);
                if (taken) return BadRequest("Ez az email cím már foglalt");
                user.Email = request.Email;
                user.EmailConfirmed = false;
            }

            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { user.Id, user.Username, user.Email });
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = _currentUser.GetUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return BadRequest(new { message = "A jelenlegi jelszó helytelen" });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Jelszó sikeresen megváltoztatva!" });
        }
    }

    namespace FortunaCasino.DTOs
    {
        public class UpdateProfileRequest
        {
            public string? Username { get; set; }
            public string? Email { get; set; }
        }
        public class ChangePasswordRequest
        {
            public string CurrentPassword { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
        }
    }
}