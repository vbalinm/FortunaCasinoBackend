using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FortunaCasino.Data;
using FortunaCasino.DTOs.Lottery;
using FortunaCasino.Models;
using FortunaCasino.Services.Interfaces;

namespace FortunaCasino.Controllers;

[ApiController]
[Route("api/lottery")]
public class LotteryController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILotteryService _lotteryService;

    public LotteryController(AppDbContext context, ILotteryService lotteryService)
    {
        _context = context;
        _lotteryService = lotteryService;
    }

    [HttpGet("draws")]
    public async Task<IActionResult> GetDraws()
    {
        var draws = await _context.LotteryDraws
            .Where(d => d.IsActive && d.DrawDate > DateTime.UtcNow)
            .Select(d => new { d.Id, d.DrawDate, d.TicketPrice, d.GameType, d.IsDrawn })
            .ToListAsync();

        return Ok(draws);
    }

    [Authorize]
    [HttpPost("ticket")]
    public async Task<IActionResult> BuyTicket([FromBody] BuyTicketRequest request)
    {
        if (request == null)
            return BadRequest("Érvénytelen kérés");

        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = await _context.Users.FirstAsync(u => u.Id == userId);
            var draw = await _context.LotteryDraws
                .FirstOrDefaultAsync(d => d.Id == request.DrawId && d.IsActive);

            if (draw == null)
            {
                await transaction.RollbackAsync(); 
                return BadRequest("Érvénytelen sorsolás");
            }

            if (string.IsNullOrWhiteSpace(draw.GameType))
            {
                await transaction.RollbackAsync();
                return BadRequest("Érvénytelen játéktípus");
            }

            if (draw.DrawDate < DateTime.UtcNow.AddHours(1))
            {
                await transaction.RollbackAsync(); 
                return BadRequest("Lejárt a vásárlási idő");
            }

            string numbers;

            if (!request.IsQuickPick)
            {
                if (string.IsNullOrWhiteSpace(request.FieldA))
                {
                    await transaction.RollbackAsync();
                    return BadRequest("Számok megadása kötelező");
                }

                // Validálás a GameType szerint
                if (!_lotteryService.ValidateNumbers(request.FieldA, draw.GameType))
                {
                    await transaction.RollbackAsync();  
                    return BadRequest("Érvénytelen számok a játéktípushoz");
                }

                numbers = request.FieldA;
            }
            else
            {
                numbers = _lotteryService.GenerateRandomNumbers(draw.GameType);
            }

            var totalPrice = draw.TicketPrice;

            if (user.Balance < totalPrice)
            {
                await transaction.RollbackAsync(); 
                return BadRequest("Nincs elég egyenleg");
            }

            user.Balance -= totalPrice;

            var ticket = new LotteryTicket
            {
                UserId = userId,
                DrawId = request.DrawId,
                TicketCode = await _lotteryService.GenerateTicketCode(_context),
                FieldA = numbers,           
                FieldsFilled = 1,        
                IsQuickPick = request.IsQuickPick,
                TotalPrice = totalPrice,
                Status = "active"           
            };

            _context.LotteryTickets.Add(ticket);

            _context.Transactions.Add(new Transaction
            {
                UserId = userId,
                Type = "ticket_purchase",
                Amount = -totalPrice,
                BalanceBefore = user.Balance + totalPrice,
                BalanceAfter = user.Balance,
                Description = $"Szelvény: {ticket.TicketCode}"
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                ticket.TicketCode,
                ticket.FieldA,
                Fields = 1,
                Price = totalPrice,
                Balance = user.Balance,
                GameType = draw.GameType
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Hiba történt a szelvény vásárlása közben. Kérjük próbálja újra később.");
        }
    }

    [Authorize]
    [HttpGet("my-tickets")]
    public async Task<IActionResult> GetMyTickets()
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var tickets = await _context.LotteryTickets
            .Where(t => t.UserId == userId)
            .Include(t => t.Draw)
            .OrderByDescending(t => t.BoughtAt)
            .Select(t => new TicketResponse
            {
                Id = t.Id,
                TicketCode = t.TicketCode,
                DrawDate = t.Draw.DrawDate,
                FieldA = t.FieldA,
                FieldB = t.FieldB,
                TotalPrice = t.TotalPrice,
                WinAmount = t.TotalWinAmount,
                Status = t.Status
            })
            .ToListAsync();

        return Ok(tickets);
    }

    [Authorize(Roles = "admin")]
    [HttpPost("draw/{id}")]
    public async Task<IActionResult> DrawLottery(long id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var draw = await _context.LotteryDraws.FindAsync(id);
            if (draw == null)
            {
                await transaction.RollbackAsync();
                return BadRequest("A sorsolás nem létezik");
            }

            if (draw.IsDrawn)
            {
                await transaction.RollbackAsync();
                return BadRequest("Már sorsoltak");
            }

            if (string.IsNullOrWhiteSpace(draw.GameType))
            {
                await transaction.RollbackAsync();
                return BadRequest("Érvénytelen játéktípus");
            }

            var winningNumbers = _lotteryService.GenerateWinningNumbers(draw.GameType);

            if (string.IsNullOrWhiteSpace(winningNumbers))
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Hiba a nyerőszámok generálása közben");
            }

            draw.WinningNumbers = winningNumbers;
            draw.IsDrawn = true;
            draw.DrawnAt = DateTime.UtcNow;

            var tickets = await _context.LotteryTickets
                .Include(t => t.User)
                .Where(t => t.DrawId == id && t.Status == "active")
                .ToListAsync();

            decimal totalPayout = 0;
            int winners = 0;

            foreach (var ticket in tickets)
            {
                decimal winAmount = 0;

                if (!string.IsNullOrWhiteSpace(ticket.FieldA))
                {
                    var matches = _lotteryService.CalculateMatches(ticket.FieldA, winningNumbers);
                    ticket.MatchesA = (byte)matches;

                    winAmount += _lotteryService.CalculatePrize(
                        matches,
                        draw.TicketPrice,
                        draw.GameType,
                        10 
                    );
                }

                ticket.TotalWinAmount = winAmount;
                ticket.Status = "drawn";

                if (winAmount > 0)
                {
                    if (ticket.User == null)
                    {
                        await transaction.RollbackAsync();
                        return StatusCode(500, "Hiba: Felhasználó nem található a szelvényhez");
                    }

                    ticket.User.Balance += winAmount;
                    ticket.IsPaidOut = true;
                    totalPayout += winAmount;
                    winners++;

                    _context.Transactions.Add(new Transaction
                    {
                        UserId = ticket.UserId,
                        Type = "win_payout",
                        Amount = winAmount,
                        BalanceBefore = ticket.User.Balance - winAmount,
                        BalanceAfter = ticket.User.Balance,
                        TicketId = ticket.Id,
                        Description = $"Nyeremény: {ticket.TicketCode} ({draw.GameType})"
                    });
                }
            }

            draw.TotalPayout = totalPayout;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                winningNumbers,
                gameType = draw.GameType,
                TotalTickets = tickets.Count,
                Winners = winners,
                TotalPayout = totalPayout
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Hiba történt a sorsolás közben. Kérjük próbálja újra később.");
        }
    }
}