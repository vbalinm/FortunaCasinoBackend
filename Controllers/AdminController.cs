using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FortunaCasino.Data;
using FortunaCasino.DTOs;
using FortunaCasino.Models;

namespace FortunaCasino.Controllers;

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

        return Ok(new
        {
            UserId = id,
            OldBalance = oldBalance,
            NewBalance = user.Balance
        });
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var totalUsers = await _context.Users.CountAsync();
        var totalBalance = await _context.Users.SumAsync(u => u.Balance);
        var totalTickets = await _context.LotteryTickets.CountAsync();
        var totalTransactions = await _context.Transactions.CountAsync();

        return Ok(new
        {
            TotalUsers = totalUsers,
            TotalBalanceInSystem = totalBalance,
            TotalTicketsSold = totalTickets,
            TotalTransactions = totalTransactions
        });
    }
}