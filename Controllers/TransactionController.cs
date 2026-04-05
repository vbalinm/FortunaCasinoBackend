using FortunaCasino.Data;
using FortunaCasino.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FortunaCasino.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public TransactionController(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyTransactions()
    {
        var userId = _currentUser.GetUserId();

        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id,
                t.Type,
                t.Amount,
                t.BalanceBefore,
                t.BalanceAfter,
                t.Description,
                t.CreatedAt
            })
            .ToListAsync();

        return Ok(transactions);
    }
}