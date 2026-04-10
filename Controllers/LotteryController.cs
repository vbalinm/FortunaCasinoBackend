using FortunaCasino.Data;
using FortunaCasino.DTOs.Lottery;
using FortunaCasino.Models;
using FortunaCasino.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using FortunaCasino.Helpers;

namespace FortunaCasino.Controllers;

[ApiController]
[Route("api")]
public class LotteryController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILotteryService _lotteryService;
    private readonly ICurrentUserService _currentUser;

    public LotteryController(AppDbContext context, ILotteryService lotteryService, ICurrentUserService currentUser)
    {
        _context = context;
        _lotteryService = lotteryService;
        _currentUser = currentUser;
    }

    [HttpGet("lottery/draws")]
    public async Task<IActionResult> GetDraws()
    {
        var draws = await _context.LotteryDraws
            .Where(d => d.IsActive && !d.IsDrawn)
            .Select(d => new { d.Id, d.DrawDate, d.TicketPrice, d.IsDrawn, d.GameType })
            .ToListAsync();

        return Ok(draws);
    }

    [Authorize]
    [HttpGet("tickets/my")]
    public async Task<IActionResult> GetMyTickets()
    {
        var userId = _currentUser.GetUserId();

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
        if (request?.Tickets == null || request.Tickets.Count == 0)
            return BadRequest(new { message = "Legalább egy szelvény szükséges" });

        if (request.Tickets.Count > 50)
            return BadRequest(new { message = "Maximum 50 szelvény küldhető egyszerre" });

        var userId = _currentUser.GetUserId();

        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return Unauthorized(new { message = "Felhasználó nem található" });

            if (!user.IsActive)
                return Forbid();

            var activeDraws = await _context.LotteryDraws
                .Where(d => d.IsActive && !d.IsDrawn)
                .ToListAsync();

            if (activeDraws.Count == 0)
                return BadRequest(new { message = "Nincs aktív sorsolás" });

            var validationErrors = new List<string>();
            decimal totalPrice = 0;

            for (int i = 0; i < request.Tickets.Count; i++)
            {
                var item = request.Tickets[i];
                var prefix = $"Szelvény #{i + 1}";

                //Ár validáció
                var priceCheck = ValidationHelper.ValidatePrice(item.Price, item.Price);
                if (item.Price <= 0)
                    validationErrors.Add($"{prefix}: Az ár nem lehet nulla vagy negatív");

                //Mennyiség validáció
                var qtyCheck = ValidationHelper.ValidateQuantity(item.Quantity);
                if (!qtyCheck.IsValid)
                    validationErrors.Add($"{prefix}: {qtyCheck.ErrorMessage}");

                //Típus validáció
                if (string.IsNullOrWhiteSpace(item.Type) ||
                    !new[] { "panel", "extra", "joker" }.Contains(item.Type))
                    validationErrors.Add($"{prefix}: Érvénytelen szelvény típus '{item.Type}'");

                // Számok validáció (ha van)
                if (item.Numbers != null && item.Type != "extra")
                {
                    var numbersStr = ExtractNumbersString(item.Numbers, item.Type);
                    var gameType = MapGameNameToType(item.GameName);
                    var numCheck = ValidationHelper.ValidateNumbers(numbersStr, gameType);
                    if (!numCheck.IsValid)
                        validationErrors.Add($"{prefix}: {numCheck.ErrorMessage}");
                }

                totalPrice += item.Price * item.Quantity;
            }

            if (validationErrors.Count > 0)
                return BadRequest(new { message = "Validációs hiba", errors = validationErrors });

            //Egyenleg ellenőrzés
            if (totalPrice <= 0)
                return BadRequest(new { message = "Érvénytelen végösszeg" });

            if (user.Balance < totalPrice)
                return BadRequest(new
                {
                    message = "Nincs elegendő egyenleg",
                    required = totalPrice,
                    available = user.Balance,
                    missing = totalPrice - user.Balance
                });

            //Szelvények létrehozása
            var createdTickets = new List<object>();

            foreach (var item in request.Tickets)
            {
                var gameType = MapGameNameToType(item.GameName);
                var draw = activeDraws.FirstOrDefault(d => d.GameType == gameType)
                            ?? activeDraws.First();

                for (int q = 0; q < item.Quantity; q++)
                {
                    var fieldsNumbers = item.Type == "extra"
                        ? _lotteryService.GenerateRandomNumbers()
                        : ExtractNumbersString(item.Numbers, item.Type);

                    int fieldCount = string.IsNullOrEmpty(fieldsNumbers)
                        ? 0
                        : fieldsNumbers.Split('|').Length;

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

            //Egyenleg levonás + tranzakció
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
            await dbTransaction.CommitAsync();

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
            await dbTransaction.RollbackAsync();
            return StatusCode(500, new { message = $"Vásárlási hiba: {ex.Message}" });
        }
    }

    //Számok kinyerése a JSON elemből
    private static string ExtractNumbersString(object? numbers, string type)
    {
        if (numbers == null) return string.Empty;

        if (type == "joker")
            return numbers is JsonElement je2 ? je2.GetString() ?? "" : numbers.ToString() ?? "";

        if (numbers is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.Array)
            {
                var nums = je.EnumerateArray()
                    .Select(x => x.GetInt32())
                    .OrderBy(n => n)
                    .ToList();
                return string.Join(";", nums);
            }
            return je.GetString() ?? "";
        }

        return numbers.ToString() ?? "";
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