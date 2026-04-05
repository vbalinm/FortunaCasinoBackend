using FortunaCasino.Controllers.FortunaCasino.DTOs;
using FortunaCasino.Data;
using FortunaCasino.Models;
using FortunaCasino.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FortunaCasino.Controllers
{ 
    [ApiController]
    [Route("api/balance")]
    [Authorize]
    public class BalanceController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public BalanceController(AppDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        [HttpPost("topup")]
        public async Task<IActionResult> TopUp([FromBody] TopUpRequest request)
        {
            if (request.Amount <= 0)
                return BadRequest(new { message = "Az összegnek pozitívnak kell lennie" });

            var userId = _currentUser.GetUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound();

            var oldBalance = user.Balance;
            user.Balance += request.Amount;

            _context.Transactions.Add(new Transaction
            {
                UserId = userId,
                Type = "deposit",
                Amount = request.Amount,
                BalanceBefore = oldBalance,
                BalanceAfter = user.Balance,
                Description = $"Egyenleg feltöltés: +{request.Amount:N0} Ft",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Sikeres feltöltés!",
                NewBalance = user.Balance,
                Added = request.Amount
            });
        }

        [HttpPost("admin/topup/{userId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AdminTopUp(long userId, [FromBody] TopUpRequest request)
        {
            if (request.Amount <= 0)
                return BadRequest(new { message = "Az összegnek pozitívnak kell lennie" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "Felhasználó nem található" });

            var adminId = _currentUser.GetUserId();
            var oldBalance = user.Balance;
            user.Balance += request.Amount;

            _context.Transactions.Add(new Transaction
            {
                UserId = userId,
                Type = "admin_topup",
                Amount = request.Amount,
                BalanceBefore = oldBalance,
                BalanceAfter = user.Balance,
                Description = $"Admin feltöltés (admin ID: {adminId}): +{request.Amount:N0} Ft",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Sikeres admin feltöltés!",
                UserId = userId,
                Username = user.Username,
                NewBalance = user.Balance,
                Added = request.Amount
            });
        }
    }

    namespace FortunaCasino.DTOs
    {
        public class TopUpRequest
        {
            public decimal Amount { get; set; }
        }
    }
}