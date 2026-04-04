using FortunaCasino.Data;
using FortunaCasino.DTOs.Lottery;           // ← Ez most már működni fog
using FortunaCasino.Models;
using FortunaCasino.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace FortunaCasino.Controllers;

[ApiController]
[Route("api")]
public class LotteryController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILotteryService _lotteryService;

    public LotteryController(AppDbContext context, ILotteryService lotteryService)
    {
        _context = context;
        _lotteryService = lotteryService;
    }

    [HttpGet("lottery/draws")]
    public async Task<IActionResult> GetDraws()
    {
        var draws = await _context.LotteryDraws
            .Where(d => d.IsActive && d.DrawDate > DateTime.UtcNow)
            .Select(d => new { d.Id, d.DrawDate, d.TicketPrice, d.IsDrawn, d.GameType })
            .ToListAsync();

        return Ok(draws);
    }

    [Authorize]
    [HttpGet("tickets/my")]
    public async Task<IActionResult> GetMyTickets()
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var tickets = await _context.LotteryTickets
            .Where(t => t.UserId == userId)
            .Include(t => t.Draw)
            .OrderByDescending(t => t.BoughtAt)
            .Select(t => new
            {
                t.Id,
                t.TicketCode,
                GameType = t.Draw.GameType,
                t.FieldsNumbers,
                t.Fields,
                t.FieldsFilled,
                t.TotalPrice,
                WinAmount = t.TotalWinAmount,
                t.Status,
                t.BoughtAt,
                DrawDate = t.Draw.DrawDate
            })
            .ToListAsync();

        return Ok(tickets);
    }

    [Authorize]
    [HttpPost("tickets/purchase")]
    public async Task<IActionResult> PurchaseTickets([FromBody] PurchaseRequest request)
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _context.Users.FirstAsync(u => u.Id == userId);

            decimal totalPrice = request.Tickets.Sum(item => item.Price * item.Quantity);

            if (user.Balance < totalPrice)
                return BadRequest(new { message = "Nincs elegendő egyenleg" });

            var activeDraws = await _context.LotteryDraws
                .Where(d => d.IsActive && !d.IsDrawn && d.DrawDate > DateTime.UtcNow)
                .ToListAsync();

            var createdTickets = new List<object>();

            foreach (var item in request.Tickets)
            {
                var gameType = MapGameNameToType(item.GameName);

                var draw = activeDraws.FirstOrDefault(d => d.GameType == gameType)
                        ?? activeDraws.FirstOrDefault();

                if (draw == null) continue;

                for (int q = 0; q < item.Quantity; q++)
                {
                    string fieldsNumbers = "";
                    int fieldCount = 1;

                    if (item.Type == "joker")
                    {
                        fieldsNumbers = item.Numbers is JsonElement je
                            ? je.GetString() ?? ""
                            : item.Numbers?.ToString() ?? "";
                    }
                    else
                    {
                        if (item.Numbers is JsonElement je)
                        {
                            if (je.ValueKind == JsonValueKind.Array)
                            {
                                var nums = je.EnumerateArray()
                                    .Select(x => x.GetInt32())
                                    .OrderBy(n => n)
                                    .ToList();
                                fieldsNumbers = string.Join(";", nums);
                            }
                            else
                            {
                                fieldsNumbers = je.GetString() ?? "";
                                fieldCount = fieldsNumbers.Split('|').Length;
                            }
                        }
                        else
                        {
                            fieldsNumbers = item.Numbers?.ToString() ?? "";
                        }
                    }

                    var ticket = new LotteryTicket
                    {
                        UserId = userId,
                        DrawId = draw.Id,
                        TicketCode = await _lotteryService.GenerateTicketCode(_context),
                        FieldsNumbers = fieldsNumbers,
                        Fields = fieldCount,
                        FieldsFilled = (byte)fieldCount,
                        IsQuickPick = item.Type == "extra",
                        TotalPrice = item.Price,
                        Status = "active",
                        BoughtAt = DateTime.UtcNow
                    };

                    _context.LotteryTickets.Add(ticket);

                    createdTickets.Add(new
                    {
                        ticket.TicketCode,
                        GameName = item.GameName,
                        item.Price,
                        FieldsNumbers = fieldsNumbers
                    });
                }
            }

            var balanceBefore = user.Balance;
            user.Balance -= totalPrice;

            _context.Transactions.Add(new Transaction
            {
                UserId = userId,
                Type = "ticket_purchase",
                Amount = -totalPrice,
                BalanceBefore = balanceBefore,
                BalanceAfter = user.Balance,
                Description = $"{createdTickets.Count} szelvény vásárlása",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                Message = "Sikeres vásárlás!",
                NewBalance = user.Balance,
                TicketsBought = createdTickets.Count,
                Tickets = createdTickets
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = $"Vásárlási hiba: {ex.Message}" });
        }
    }
    private static string MapGameNameToType(string? gameName)
        => (gameName?.ToLower() ?? "") switch
        {
            var n when n.Contains("ötös") || n.Contains("otos") => "Lottery5",
            var n when n.Contains("hatos") => "Lottery6",
            var n when n.Contains("skandináv") || n.Contains("skandi") => "Scandinavian",
            var n when n.Contains("eurojackpot") || n.Contains("euro") => "Eurojackpot",
            var n when n.Contains("joker") => "Joker",
            var n when n.Contains("kenó") || n.Contains("keno") => "Keno",
            _ => "Lottery5"
        };
}