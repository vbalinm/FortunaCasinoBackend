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
    private readonly IEmailService _emailService;

    public LotteryController(AppDbContext context, ILotteryService lotteryService, ICurrentUserService currentUser, IEmailService emailService)
    {
        _context = context;
        _lotteryService = lotteryService;
        _currentUser = currentUser;
        _emailService = emailService;
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

                if (item.Price <= 0)
                    validationErrors.Add($"{prefix}: Az ár nem lehet nulla vagy negatív");

                var qtyCheck = ValidationHelper.ValidateQuantity(item.Quantity);
                if (!qtyCheck.IsValid)
                    validationErrors.Add($"{prefix}: {qtyCheck.ErrorMessage}");

                if (string.IsNullOrWhiteSpace(item.Type) ||
                    !new[] { "panel", "extra", "joker" }.Contains(item.Type))
                    validationErrors.Add($"{prefix}: Érvénytelen szelvény típus '{item.Type}'");

                if (item.Numbers != null && item.Type != "extra" &&
                    MapGameNameToType(item.GameName) != "Keno")
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

            // Összes szükséges ticket kód előre generálása
            int totalTicketsNeeded = request.Tickets.Sum(item => item.Quantity);
            var ticketCodes = new List<string>();

            for (int i = 0; i < totalTicketsNeeded; i++)
            {
                string code;
                do
                {
                    var codePrefix = $"LOT{DateTime.Now:yyMM}";
                    var lastTicket = await _context.LotteryTickets
                        .Where(t => t.TicketCode.StartsWith(codePrefix))
                        .OrderByDescending(t => t.TicketCode)
                        .FirstOrDefaultAsync();

                    int sequence = 1;
                    if (lastTicket != null)
                    {
                        var lastSeq = lastTicket.TicketCode[9..];
                        if (int.TryParse(lastSeq, out var seq))
                            sequence = seq + 1;
                    }
                    sequence += ticketCodes.Count;
                    code = $"{codePrefix}{sequence:D5}";
                } while (ticketCodes.Contains(code) ||
                         await _context.LotteryTickets.AnyAsync(t => t.TicketCode == code));

                ticketCodes.Add(code);
            }

            // Szelvények létrehozása
            var createdTickets = new List<(string TicketCode, string GameName, decimal Price, string FieldsNumbers)>();

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
                        TicketCode = ticketCodes[createdTickets.Count],
                        FieldsNumbers = fieldsNumbers,
                        Fields = fieldCount,
                        FieldsFilled = (byte)fieldCount,
                        IsQuickPick = item.Type == "extra",
                        TotalPrice = item.Price,
                        Status = "active",
                        BoughtAt = DateTime.Now
                    };

                    _context.LotteryTickets.Add(ticket);
                    createdTickets.Add((ticketCodes[createdTickets.Count], item.GameName ?? "", item.Price, fieldsNumbers));
                }
            }

            // Egyenleg levonás + tranzakció
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
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            //Vásárlási visszaigazoló email
            try
            {
                var ticketRows = string.Join("", createdTickets.Select(t => $@"
                    <tr>
                        <td style='padding:8px;border-bottom:1px solid #eee;font-family:monospace;'>{t.TicketCode}</td>
                        <td style='padding:8px;border-bottom:1px solid #eee;'>{t.GameName}</td>
                        <td style='padding:8px;border-bottom:1px solid #eee;'>{t.FieldsNumbers}</td>
                        <td style='padding:8px;border-bottom:1px solid #eee;text-align:right;'>{t.Price:N0} Ft</td>
                    </tr>"));

                var emailBody = $@"
                    <div style='font-family:Segoe UI,sans-serif;max-width:600px;margin:0 auto;'>
                        <div style='background:linear-gradient(135deg,#f59e0b,#ea580c);padding:30px;border-radius:12px 12px 0 0;text-align:center;'>
                            <h1 style='color:white;margin:0;'>🎟️ Sikeres vásárlás!</h1>
                            <p style='color:rgba(255,255,255,0.9);margin:8px 0 0;'>Fortuna Lotto</p>
                        </div>
                        <div style='background:#fff;padding:30px;border-radius:0 0 12px 12px;box-shadow:0 4px 20px rgba(0,0,0,0.1);'>
                            <p>Kedves <strong>{user.Username}</strong>!</p>
                            <p>Sikeresen leadtad a szelvényeidet. Összefoglaló:</p>

                            <table style='width:100%;border-collapse:collapse;margin:20px 0;'>
                                <thead>
                                    <tr style='background:#f9f9f9;'>
                                        <th style='padding:10px;text-align:left;border-bottom:2px solid #eee;'>Kód</th>
                                        <th style='padding:10px;text-align:left;border-bottom:2px solid #eee;'>Játék</th>
                                        <th style='padding:10px;text-align:left;border-bottom:2px solid #eee;'>Számok</th>
                                        <th style='padding:10px;text-align:right;border-bottom:2px solid #eee;'>Ár</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {ticketRows}
                                </tbody>
                            </table>

                            <div style='background:#f0fdf4;border:1px solid #bbf7d0;border-radius:8px;padding:15px;margin:20px 0;'>
                                <p style='margin:0;color:#16a34a;font-weight:bold;'>
                                    💰 Levont összeg: {totalPrice:N0} Ft<br/>
                                    💳 Új egyenleg: {user.Balance:N0} Ft
                                </p>
                            </div>

                            <p style='color:#888;font-size:13px;'>Sok szerencsét kívánunk! 🍀</p>
                            <p style='color:#888;font-size:13px;'>— Fortuna Lotto csapata</p>
                        </div>
                    </div>";

                await _emailService.SendEmailAsync(
                    user.Email,
                    $"🎟️ Fortuna Lotto - Sikeres vásárlás ({createdTickets.Count} szelvény)",
                    emailBody
                );
            }
            catch { /* Email hiba nem akasztja meg a vásárlást */ }

            return Ok(new
            {
                Message = "Sikeres vásárlás!",
                NewBalance = user.Balance,
                TicketsBought = createdTickets.Count,
                Tickets = createdTickets.Select(t => new
                {
                    t.TicketCode,
                    t.GameName,
                    t.Price,
                    FieldsNumbers = t.FieldsNumbers
                })
            });
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            return StatusCode(500, new { message = $"Vásárlási hiba: {ex.Message}" });
        }
    }

    // Számok kinyerése a JSON elemből
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