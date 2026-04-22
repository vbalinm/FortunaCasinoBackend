using FortunaCasino.Controllers.FortunaCasino.DTOs;
using FortunaCasino.Data;
using FortunaCasino.Models;
using FortunaCasino.Services;
using FortunaCasino.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FortunaCasino.Controllers
{
    [ApiController]
    [Route("api/admin/draws")]
    [Authorize(Roles = "admin")]
    public class DrawController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILotteryService _lotteryService;
        private readonly ICurrentUserService _currentUser;
        private readonly IEmailService _emailService;

        public DrawController(AppDbContext context, ILotteryService lotteryService, ICurrentUserService currentUser, IEmailService emailService)
        {
            _context = context;
            _lotteryService = lotteryService;
            _currentUser = currentUser;
            _emailService = emailService;
        }

        // Sorsolások listája (szűrésekkel)
        [HttpGet]
        public async Task<IActionResult> GetDraws(
            [FromQuery] string? gameType = null,
            [FromQuery] bool? isDrawn = null,
            [FromQuery] bool? isActive = null)
        {
            var query = _context.LotteryDraws.AsQueryable();

            if (!string.IsNullOrWhiteSpace(gameType))
                query = query.Where(d => d.GameType == gameType);

            if (isDrawn.HasValue)
                query = query.Where(d => d.IsDrawn == isDrawn.Value);

            if (isActive.HasValue)
                query = query.Where(d => d.IsActive == isActive.Value);

            var draws = await query
                .OrderByDescending(d => d.DrawDate)
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
                    TicketCount = _context.LotteryTickets.Count(t => t.DrawId == d.Id)
                })
                .ToListAsync();

            return Ok(draws);
        }

        // Egy sorsolás részletei + nyertesek
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDraw(long id)
        {
            var draw = await _context.LotteryDraws.FindAsync(id);
            if (draw == null) return NotFound(new { message = "Sorsolás nem található" });

            var tickets = await _context.LotteryTickets
                .Include(t => t.User)
                .Where(t => t.DrawId == id)
                .Select(t => new
                {
                    t.Id,
                    t.TicketCode,
                    Username = t.User.Username,
                    t.FieldsNumbers,
                    t.TotalPrice,
                    t.TotalWinAmount,
                    t.Status,
                    t.MatchesNumbers,
                    t.BoughtAt
                })
                .OrderByDescending(t => t.TotalWinAmount)
                .ToListAsync();

            var winners = tickets.Where(t => t.TotalWinAmount > 0).ToList();

            return Ok(new
            {
                Draw = new
                {
                    draw.Id,
                    draw.GameType,
                    draw.DrawDate,
                    draw.TicketPrice,
                    draw.IsDrawn,
                    draw.IsActive,
                    draw.WinningNumbers,
                    draw.TotalTicketsSold,
                    draw.TotalPayout,
                    draw.DrawnAt
                },
                TotalTickets = tickets.Count,
                WinnerCount = winners.Count,
                TotalPayout = winners.Sum(w => w.TotalWinAmount),
                Winners = winners,
                AllTickets = tickets
            });
        }

        // Új sorsolás létrehozása
        [HttpPost("create")]
        public async Task<IActionResult> CreateDraw([FromBody] CreateDrawRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.GameType))
                return BadRequest(new { message = "Játéktípus megadása kötelező" });

            if (request.DrawDate <= DateTime.Now)
                return BadRequest(new { message = "A sorsolás dátuma jövőbeli kell legyen" });

            if (request.TicketPrice <= 0)
                return BadRequest(new { message = "Az ár nem lehet nulla vagy negatív" });

            var draw = new LotteryDraw
            {
                GameType = request.GameType,
                DrawDate = request.DrawDate,
                TicketPrice = request.TicketPrice,
                IsActive = true,
                IsDrawn = false,
                CreatedAt = DateTime.Now
            };

            _context.LotteryDraws.Add(draw);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Sorsolás sikeresen létrehozva!",
                drawId = draw.Id,
                gameType = draw.GameType,
                drawDate = draw.DrawDate
            });
        }

        // Sorsolás aktiválása / deaktiválása
        [HttpPut("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(long id)
        {
            var draw = await _context.LotteryDraws.FindAsync(id);
            if (draw == null) return NotFound(new { message = "Sorsolás nem található" });
            if (draw.IsDrawn) return BadRequest(new { message = "Már lezajlott sorsolást nem lehet módosítani" });

            draw.IsActive = !draw.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { drawId = id, isActive = draw.IsActive });
        }

        // Sorsolás törlése
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDraw(long id)
        {
            var draw = await _context.LotteryDraws.FindAsync(id);
            if (draw == null)
                return NotFound(new { message = "Sorsolás nem található" });

            if (draw.IsDrawn)
                return BadRequest(new { message = "Lezárt sorsolást nem lehet törölni" });

            var tickets = await _context.LotteryTickets
                .Where(t => t.DrawId == id)
                .ToListAsync();

            if (tickets.Any())
                _context.LotteryTickets.RemoveRange(tickets);

            _context.LotteryDraws.Remove(draw);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Sorsolás sikeresen törölve!" });
        }

        // Sorsolás végrehajtása
        [HttpPost("{id}/execute")]
        public async Task<IActionResult> ExecuteDraw(long id)
        {
            var draw = await _context.LotteryDraws.FindAsync(id);

            if (draw == null)
                return NotFound(new { message = "Sorsolás nem található" });

            if (draw.IsDrawn)
                return BadRequest(new { message = "Ez a sorsolás már le lett húzva" });

            if (!draw.IsActive)
                return BadRequest(new { message = "Inaktív sorsolást nem lehet végrehajtani" });

            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var adminId = _currentUser.GetUserId();
                var winningNumbers = _lotteryService.GenerateNumbersForGame(draw.GameType);

                draw.WinningNumbers = winningNumbers;
                draw.IsDrawn = true;
                draw.DrawnAt = DateTime.Now;
                draw.DrawnBy = adminId;

                var tickets = await _context.LotteryTickets
                    .Include(t => t.User)
                    .Where(t => t.DrawId == id && t.Status == "active")
                    .ToListAsync();

                decimal totalPayout = 0;
                int winnerCount = 0;
                var winnerList = new List<object>();

                foreach (var ticket in tickets)
                {
                    decimal winAmount = 0;
                    int totalMatches = 0;

                    if (!string.IsNullOrEmpty(ticket.FieldsNumbers))
                    {
                        var fields = ticket.FieldsNumbers.Split('|', StringSplitOptions.RemoveEmptyEntries);

                        foreach (var field in fields)
                        {
                            var matches = _lotteryService.CalculateMatchesForGame(field, winningNumbers, draw.GameType);

                            var bonusMatches = draw.GameType == "Eurojackpot"
                                ? ((LotteryService)_lotteryService).CalculateBonusMatchesForGame(field, winningNumbers, draw.GameType)
                                : 0;

                            var fieldPrize = _lotteryService.CalculatePrizeForGame(matches, bonusMatches, draw.GameType, draw.TicketPrice);
                            winAmount += fieldPrize;
                            totalMatches = Math.Max(totalMatches, matches);
                        }
                    }

                    ticket.MatchesNumbers = (byte)totalMatches;
                    ticket.TotalWinAmount = winAmount;
                    ticket.Status = "drawn";

                    if (winAmount > 0)
                    {
                        ticket.IsPaidOut = true;
                        ticket.User.Balance += winAmount;
                        totalPayout += winAmount;
                        winnerCount++;

                        _context.Transactions.Add(new Transaction
                        {
                            UserId = ticket.UserId,
                            Type = "win_payout",
                            Amount = winAmount,
                            BalanceBefore = ticket.User.Balance - winAmount,
                            BalanceAfter = ticket.User.Balance,
                            TicketId = ticket.Id,
                            Description = $"Nyeremény: {ticket.TicketCode} ({draw.GameType})",
                            CreatedAt = DateTime.Now
                        });

                        winnerList.Add(new
                        {
                            ticket.TicketCode,
                            Username = ticket.User.Username,
                            ticket.FieldsNumbers,
                            Matches = totalMatches,
                            WinAmount = winAmount
                        });

                        await _emailService.SendEmailAsync(
                            ticket.User.Email,
                            "🎉 Nyertél a FortunaCasinón!",
                            $@"
                            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                                <h2 style='color: #ea580c;'>🎉 Gratulálunk, {ticket.User.Username}!</h2>
                                <p>Nyertél a <strong>{draw.GameType}</strong> sorsoláson!</p>
                                <div style='background: #f3f4f6; border-radius: 8px; padding: 16px; margin: 16px 0;'>
                                    <p><strong>Szelvénykód:</strong> {ticket.TicketCode}</p>
                                    <p><strong>Találatok:</strong> {totalMatches}</p>
                                    <p><strong>Nyeremény:</strong> {winAmount.ToString("N0")} Ft</p>
                                </div>
                                <p>A nyeremény már jóvá lett írva az egyenlegedre. 🏆</p>
                                <p style='color: #6b7280; font-size: 12px;'>FortunaCasino csapata</p>
                            </div>"
                        );
                    }
                }

                draw.TotalTicketsSold = tickets.Count;
                draw.TotalPayout = totalPayout;

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return Ok(new
                {
                    Message = "Sorsolás sikeresen végrehajtva!",
                    DrawId = id,
                    GameType = draw.GameType,
                    WinningNumbers = winningNumbers,
                    TotalTickets = tickets.Count,
                    WinnerCount = winnerCount,
                    TotalPayout = totalPayout,
                    Winners = winnerList
                });
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return StatusCode(500, new { message = $"Sorsolási hiba: {ex.Message}" });
            }
        }

        // Sorsolás eredményei
        [HttpGet("{id}/results")]
        public async Task<IActionResult> GetResults(long id)
        {
            var draw = await _context.LotteryDraws.FindAsync(id);
            if (draw == null) return NotFound(new { message = "Sorsolás nem található" });
            if (!draw.IsDrawn) return BadRequest(new { message = "Ez a sorsolás még nem lett végrehajtva" });

            var winners = await _context.LotteryTickets
                .Include(t => t.User)
                .Where(t => t.DrawId == id && t.TotalWinAmount > 0)
                .OrderByDescending(t => t.TotalWinAmount)
                .Select(t => new
                {
                    t.TicketCode,
                    Username = t.User.Username,
                    t.FieldsNumbers,
                    t.MatchesNumbers,
                    t.TotalWinAmount,
                    t.IsPaidOut
                })
                .ToListAsync();

            return Ok(new
            {
                DrawId = id,
                GameType = draw.GameType,
                DrawDate = draw.DrawDate,
                DrawnAt = draw.DrawnAt,
                WinningNumbers = draw.WinningNumbers,
                TotalTickets = draw.TotalTicketsSold,
                WinnerCount = winners.Count,
                TotalPayout = draw.TotalPayout,
                Winners = winners
            });
        }
    }

    namespace FortunaCasino.DTOs
    {
        public class CreateDrawRequest
        {
            public string GameType { get; set; } = string.Empty;
            public DateTime DrawDate { get; set; }
            public decimal TicketPrice { get; set; }
        }
    }
}