using Microsoft.EntityFrameworkCore;
using FortunaCasino.Data;
using FortunaCasino.Services.Interfaces;

namespace FortunaCasino.Services;

public class LotteryService : ILotteryService
{
    private readonly Random _random = new();

    public bool ValidateNumbers(string numbers)
    {
        try
        {
            var nums = numbers.Split(';').Select(int.Parse).ToList();
            if (nums.Count != 5) return false;
            if (nums.Any(n => n < 1 || n > 90)) return false;
            if (nums.Distinct().Count() != 5) return false;

            var sorted = nums.OrderBy(n => n).ToList();
            return nums.SequenceEqual(sorted);
        }
        catch
        {
            return false;
        }
    }

    public string GenerateRandomNumbers()
    {
        var numbers = new List<int>();
        while (numbers.Count < 5)
        {
            var num = _random.Next(1, 91);
            if (!numbers.Contains(num))
                numbers.Add(num);
        }
        return string.Join(";", numbers.OrderBy(n => n));
    }

    public int CalculateMatches(string ticketNumbers, string winningNumbers)
    {
        var ticket = ticketNumbers.Split(';').Select(int.Parse).ToList();
        var winning = winningNumbers.Split(';').Select(int.Parse).ToList();
        return ticket.Intersect(winning).Count();
    }

    public decimal CalculatePrize(int matches, decimal ticketPrice)
    {
        return matches switch
        {
            5 => ticketPrice * 10000,
            4 => ticketPrice * 100,
            3 => ticketPrice * 10,
            2 => ticketPrice * 2,
            _ => 0
        };
    }

    public async Task<string> GenerateTicketCode(AppDbContext context)
    {
        var prefix = $"LOT{DateTime.Now:yyMM}";
        var lastTicket = await context.LotteryTickets
            .Where(t => t.TicketCode.StartsWith(prefix))
            .OrderByDescending(t => t.TicketCode)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastTicket != null)
        {
            var lastSequence = lastTicket.TicketCode.Substring(9);
            if (int.TryParse(lastSequence, out var seq))
                sequence = seq + 1;
        }

        return $"{prefix}{sequence:D5}";
    }
}