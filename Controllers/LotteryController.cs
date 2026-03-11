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
            .Select(d => new { d.Id, d.DrawDate, d.TicketPrice, d.IsDrawn })
            .ToListAsync();

        return Ok(draws);
    }

    [Authorize]
    [HttpPost("ticket")]
    public async Task<IActionResult> BuyTicket([FromBody] BuyTicketRequest request)
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = await _context.Users.FirstAsync(u => u.Id == userId);
            var draw = await _context.LotteryDraws.FindAsync(request.DrawId);

            if (draw == null || !draw.IsActive)
                return BadRequest("Érvénytelen sorsolás");
            if (draw.DrawDate < DateTime.UtcNow.AddHours(1))
                return BadRequest("Lejárt a vásárlási idő");

            var fields = new List<string?>();

            if (request.IsQuickPick)
            {
                fields.Add(_lotteryService.GenerateRandomNumbers());
                if (!string.IsNullOrEmpty(request.FieldB)) fields.Add(_lotteryService.GenerateRandomNumbers());
                if (!string.IsNullOrEmpty(request.FieldC)) fields.Add(_lotteryService.GenerateRandomNumbers());
                if (!string.IsNullOrEmpty(request.FieldD)) fields.Add(_lotteryService.GenerateRandomNumbers());
                if (!string.IsNullOrEmpty(request.FieldE)) fields.Add(_lotteryService.GenerateRandomNumbers());
                if (!string.IsNullOrEmpty(request.FieldF)) fields.Add(_lotteryService.GenerateRandomNumbers());
            }
            else
            {
                if (!string.IsNullOrEmpty(request.FieldA)) fields.Add(request.FieldA);
                if (!string.IsNullOrEmpty(request.FieldB)) fields.Add(request.FieldB);
                if (!string.IsNullOrEmpty(request.FieldC)) fields.Add(request.FieldC);
                if (!string.IsNullOrEmpty(request.FieldD)) fields.Add(request.FieldD);
                if (!string.IsNullOrEmpty(request.FieldE)) fields.Add(request.FieldE);
                if (!string.IsNullOrEmpty(request.FieldF)) fields.Add(request.FieldF);
            }

            if (fields.Count == 0)
                return BadRequest("Legalább egy mező kötelező");

            foreach (var field in fields)
            {
                if (!_lotteryService.ValidateNumbers(field!))
                    return BadRequest("Érvénytelen számformátum");
            }

            var totalPrice = draw.TicketPrice * fields.Count;

            if (user.Balance < totalPrice)
                return BadRequest("Nincs elég egyenleg");

            user.Balance -= totalPrice;

            var ticket = new LotteryTicket
            {
                UserId = userId,
                DrawId = request.DrawId,
                TicketCode = await _lotteryService.GenerateTicketCode(_context),
                FieldA = fields.Count > 0 ? fields[0] : null,
                FieldB = fields.Count > 1 ? fields[1] : null,
                FieldC = fields.Count > 2 ? fields[2] : null,
                FieldD = fields.Count > 3 ? fields[3] : null,
                FieldE = fields.Count > 4 ? fields[4] : null,
                FieldF = fields.Count > 5 ? fields[5] : null,
                FieldsFilled = (byte)fields.Count,
                IsQuickPick = request.IsQuickPick,
                TotalPrice = totalPrice
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
                ticket.FieldB,
                Fields = fields.Count,
                Price = totalPrice,
                Balance = user.Balance
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Hiba: {ex.Message}");
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
            if (draw == null || draw.IsDrawn)
                return BadRequest("Már sorsoltak");

            var winningNumbers = _lotteryService.GenerateRandomNumbers();
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

                if (!string.IsNullOrEmpty(ticket.FieldA))
                {
                    var m = _lotteryService.CalculateMatches(ticket.FieldA, winningNumbers);
                    ticket.MatchesA = (byte)m;
                    winAmount += _lotteryService.CalculatePrize(m, draw.TicketPrice);
                }
                if (!string.IsNullOrEmpty(ticket.FieldB))
                {
                    var m = _lotteryService.CalculateMatches(ticket.FieldB, winningNumbers);
                    ticket.MatchesB = (byte)m;
                    winAmount += _lotteryService.CalculatePrize(m, draw.TicketPrice);
                }
                if (!string.IsNullOrEmpty(ticket.FieldC))
                {
                    var m = _lotteryService.CalculateMatches(ticket.FieldC, winningNumbers);
                    ticket.MatchesC = (byte)m;
                    winAmount += _lotteryService.CalculatePrize(m, draw.TicketPrice);
                }
                if (!string.IsNullOrEmpty(ticket.FieldD))
                {
                    var m = _lotteryService.CalculateMatches(ticket.FieldD, winningNumbers);
                    ticket.MatchesD = (byte)m;
                    winAmount += _lotteryService.CalculatePrize(m, draw.TicketPrice);
                }
                if (!string.IsNullOrEmpty(ticket.FieldE))
                {
                    var m = _lotteryService.CalculateMatches(ticket.FieldE, winningNumbers);
                    ticket.MatchesE = (byte)m;
                    winAmount += _lotteryService.CalculatePrize(m, draw.TicketPrice);
                }
                if (!string.IsNullOrEmpty(ticket.FieldF))
                {
                    var m = _lotteryService.CalculateMatches(ticket.FieldF, winningNumbers);
                    ticket.MatchesF = (byte)m;
                    winAmount += _lotteryService.CalculatePrize(m, draw.TicketPrice);
                }

                ticket.TotalWinAmount = winAmount;
                ticket.Status = "drawn";

                if (winAmount > 0)
                {
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
                        Description = $"Nyeremény: {ticket.TicketCode}"
                    });
                }
            }

            draw.TotalPayout = totalPayout;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                winningNumbers,
                TotalTickets = tickets.Count,
                Winners = winners,
                TotalPayout = totalPayout
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Hiba: {ex.Message}");
        }
    }
}