using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FortunaCasino.Data;
using FortunaCasino.DTOs;
using FortunaCasino.Models;

namespace FortunaCasino.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        //Statisztikák
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var totalTickets = await _context.LotteryTickets.CountAsync();
            var totalRevenue = await _context.Transactions
                .Where(t => t.Type == "ticket_purchase")
                .SumAsync(t => -t.Amount);

            var todayStart = DateTime.UtcNow.Date;
            var todayRevenue = await _context.Transactions
                .Where(t => t.Type == "ticket_purchase" && t.CreatedAt >= todayStart)
                .SumAsync(t => -t.Amount);

            return Ok(new
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalTickets = totalTickets,
                TotalRevenue = totalRevenue,
                TodayRevenue = todayRevenue
            });
        }

        //Összes felhasználó
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Balance = u.Balance,
                    IsActive = u.IsActive,
                    Status = u.IsActive ? "active" : "banned",
                    CreatedAt = u.CreatedAt,
                    Roles = u.UserRoles.Select(ur => ur.Role.RoleName).ToList()
                })
                .ToListAsync();

            return Ok(users);
        }

        //Ban / Unban
        [HttpPut("users/{id}/ban")]
        public async Task<IActionResult> BanUser(long id, [FromBody] BanUserRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Felhasználó nem található");

            user.IsActive = request.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                UserId = id,
                IsActive = user.IsActive,
                Status = user.IsActive ? "active" : "banned"
            });
        }

        //Összes szelvény
        [HttpGet("tickets")]
        public async Task<IActionResult> GetAllTickets([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var tickets = await _context.LotteryTickets
                .Include(t => t.User)
                .Include(t => t.Draw)
                .OrderByDescending(t => t.BoughtAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    Id = t.Id,
                    TicketCode = t.TicketCode,
                    Username = t.User.Username,
                    GameType = t.Draw != null ? t.Draw.GameType : "Lottery5",
                    FieldsNumbers = t.FieldsNumbers,
                    TotalPrice = t.TotalPrice,
                    Status = t.Status,
                    BoughtAt = t.BoughtAt
                })
                .ToListAsync();

            return Ok(tickets);
        }

        //Demo egyenleg feltöltés
        [HttpPost("users/{id}/add-balance")]
        public async Task<IActionResult> AddDemoBalance(long id, [FromBody] AddBalanceRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Felhasználó nem található");

            if (request.Amount <= 0 || request.Amount > 100000)
                return BadRequest("Összeg 1 és 100 000 Ft között lehet");

            var oldBalance = user.Balance;
            user.Balance += request.Amount;

            _context.Transactions.Add(new Transaction
            {
                UserId = id,
                Type = "demo_topup",
                Amount = request.Amount,
                BalanceBefore = oldBalance,
                BalanceAfter = user.Balance,
                Description = $"Demo feltöltés: +{request.Amount} Ft",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new { UserId = id, OldBalance = oldBalance, NewBalance = user.Balance });
        }
    }
}

namespace FortunaCasino.DTOs
{
    public class BanUserRequest
    {
        public bool IsActive { get; set; }
    }
}