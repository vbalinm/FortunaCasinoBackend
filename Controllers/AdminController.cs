using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FortunaCasino.Data;
using FortunaCasino.DTOs;
using FortunaCasino.Models;
using FortunaCasino.Services.Interfaces;

namespace FortunaCasino.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public AdminController(AppDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        //Statisztikák
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var now = DateTime.Now;
            var todayStart = now.Date;
            var weekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var newToday = await _context.Users.CountAsync(u => u.CreatedAt >= todayStart);
            var newThisWeek = await _context.Users.CountAsync(u => u.CreatedAt >= weekStart);
            var totalTickets = await _context.LotteryTickets.CountAsync();
            var activeTickets = await _context.LotteryTickets.CountAsync(t => t.Status == "active");
            var ticketsToday = await _context.LotteryTickets.CountAsync(t => t.BoughtAt >= todayStart);

            var totalRevenue = await _context.Transactions
                .Where(t => t.Type == "ticket_purchase").SumAsync(t => -t.Amount);
            var todayRevenue = await _context.Transactions
                .Where(t => t.Type == "ticket_purchase" && t.CreatedAt >= todayStart).SumAsync(t => -t.Amount);
            var weekRevenue = await _context.Transactions
                .Where(t => t.Type == "ticket_purchase" && t.CreatedAt >= weekStart).SumAsync(t => -t.Amount);
            var monthRevenue = await _context.Transactions
                .Where(t => t.Type == "ticket_purchase" && t.CreatedAt >= monthStart).SumAsync(t => -t.Amount);
            var totalPayouts = await _context.Transactions
                .Where(t => t.Type == "win_payout").SumAsync(t => t.Amount);
            var totalBalance = await _context.Users.SumAsync(u => u.Balance);

            var gameStats = await _context.LotteryTickets
                .Include(t => t.Draw)
                .GroupBy(t => t.Draw.GameType)
                .Select(g => new { GameType = g.Key, Count = g.Count(), Revenue = g.Sum(t => t.TotalPrice) })
                .OrderByDescending(g => g.Count)
                .ToListAsync();

            var monthlyRevenue = await _context.Transactions
                .Where(t => t.Type == "ticket_purchase" && t.CreatedAt >= now.AddMonths(-6))
                .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Revenue = g.Sum(t => -t.Amount), Count = g.Count() })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync();

            //Sorsolási statisztikák
            var totalDraws = await _context.LotteryDraws.CountAsync();
            var completedDraws = await _context.LotteryDraws.CountAsync(d => d.IsDrawn);
            var activeDraws = await _context.LotteryDraws.CountAsync(d => d.IsActive && !d.IsDrawn);
            var pendingDraws = await _context.LotteryDraws
                .Where(d => !d.IsDrawn && d.IsActive && d.DrawDate <= DateTime.Now)
                .CountAsync();

            return Ok(new
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                NewToday = newToday,
                NewThisWeek = newThisWeek,
                TotalTickets = totalTickets,
                ActiveTickets = activeTickets,
                TicketsToday = ticketsToday,
                TotalRevenue = totalRevenue,
                TodayRevenue = todayRevenue,
                WeekRevenue = weekRevenue,
                MonthRevenue = monthRevenue,
                TotalPayouts = totalPayouts,
                TotalBalance = totalBalance,
                GameStats = gameStats,
                MonthlyRevenue = monthlyRevenue,
                TotalDraws = totalDraws,
                CompletedDraws = completedDraws,
                ActiveDraws = activeDraws,
                PendingDraws = pendingDraws
            });
        }

        //Felhasználók
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] string? search = null)
        {
            var query = _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.Username.Contains(search) || u.Email.Contains(search));

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Balance,
                    u.IsActive,
                    Status = u.IsActive ? "active" : "banned",
                    u.CreatedAt,
                    u.LastLoginAt,
                    u.EmailConfirmed,
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
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { UserId = id, IsActive = user.IsActive, Status = user.IsActive ? "active" : "banned" });
        }

        //Szelvények
        [HttpGet("tickets")]
        public async Task<IActionResult> GetAllTickets([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var tickets = await _context.LotteryTickets
                .Include(t => t.User).Include(t => t.Draw)
                .OrderByDescending(t => t.BoughtAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(t => new
                {
                    t.Id,
                    t.TicketCode,
                    Username = t.User.Username,
                    GameType = t.Draw != null ? t.Draw.GameType : "Lottery5",
                    t.FieldsNumbers,
                    t.TotalPrice,
                    t.TotalWinAmount,
                    t.Status,
                    t.BoughtAt
                })
                .ToListAsync();

            return Ok(tickets);
        }

        //Admin egyenleg feltöltés
        [HttpPost("users/{id}/add-balance")]
        public async Task<IActionResult> AddDemoBalance(long id, [FromBody] AddBalanceRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Felhasználó nem található");
            if (request.Amount <= 0) return BadRequest("Az összegnek pozitívnak kell lennie");

            var adminId = _currentUser.GetUserId();
            var oldBalance = user.Balance;
            user.Balance += request.Amount;

            _context.Transactions.Add(new Transaction
            {
                UserId = id,
                Type = "admin_topup",
                Amount = request.Amount,
                BalanceBefore = oldBalance,
                BalanceAfter = user.Balance,
                Description = $"Admin feltöltés (admin: {adminId}): +{request.Amount:N0} Ft",
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return Ok(new { UserId = id, OldBalance = oldBalance, NewBalance = user.Balance });
        }

        //Sorsolások összefoglalója
        [HttpGet("draws/summary")]
        public async Task<IActionResult> GetDrawsSummary()
        {
            var draws = await _context.LotteryDraws
                .OrderByDescending(d => d.DrawDate)
                .Take(20)
                .Select(d => new
                {
                    d.Id,
                    d.GameType,
                    d.DrawDate,
                    d.TicketPrice,
                    d.IsDrawn,
                    d.IsActive,
                    d.WinningNumbers,
                    d.TotalTicketsSold,
                    d.TotalPayout,
                    d.DrawnAt,
                    TicketCount = _context.LotteryTickets.Count(t => t.DrawId == d.Id),
                    CanExecute = !d.IsDrawn && d.IsActive
                })
                .ToListAsync();

            return Ok(draws);
        }
        // Zárolás állapotának lekérése
        [HttpGet("settings/draw-lock")]
        public async Task<IActionResult> GetDrawLock()
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == "draw_locked");
            var isLocked = setting?.Value == "true";
            return Ok(new { isLocked });
        }

        // Zárolás toggle
        [HttpPost("settings/draw-lock/toggle")]
        public async Task<IActionResult> ToggleDrawLock()
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == "draw_locked");

            if (setting == null)
            {
                setting = new SystemSetting { Key = "draw_locked", Value = "true", UpdatedAt = DateTime.Now };
                _context.SystemSettings.Add(setting);
            }
            else
            {
                setting.Value = setting.Value == "true" ? "false" : "true";
                setting.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return Ok(new { isLocked = setting.Value == "true" });
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