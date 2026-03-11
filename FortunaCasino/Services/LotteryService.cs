using Microsoft.EntityFrameworkCore;
using FortunaCasino.Data;
using FortunaCasino.Services.Interfaces;

namespace FortunaCasino.Services;

public class LotteryService : ILotteryService
{
    // JAVÍTVA: Random helyett Random.Shared használata (thread-safe)
    // A Random.Shared .NET 6+ verzióktól elérhető és thread-safe

    public bool ValidateNumbers(string numbers, string gameType)
    {
        // JAVÍTVA: Null és üres string ellenőrzés
        if (string.IsNullOrWhiteSpace(numbers))
            return false;

        if (string.IsNullOrWhiteSpace(gameType))
            return false;

        try
        {
            var nums = numbers.Split(';').Select(int.Parse).ToList();

            // Hány szám kell a játékoshoz?
            var requiredCount = gameType switch
            {
                "Lottery6" => 6,           // 6-os lottó: 6 szám
                "Keno" => 10,               // Kenó: 10 szám (fix)
                _ => 5                      // 5-ös lottó: 5 szám
            };

            // Intervallum
            var (minNum, maxNum) = gameType switch
            {
                "Lottery6" => (1, 45),      // 6-os: 1-45
                "Keno" => (1, 80),          // Kenó: 1-80
                _ => (1, 90)                // 5-ös: 1-90
            };

            if (nums.Count != requiredCount) return false;
            if (nums.Any(n => n < minNum || n > maxNum)) return false;
            if (nums.Distinct().Count() != requiredCount) return false;

            var sorted = nums.OrderBy(n => n).ToList();
            return nums.SequenceEqual(sorted);
        }
        catch
        {
            return false;
        }
    }

    public string GenerateRandomNumbers(string gameType)
    {
        // JAVÍTVA: GameType validálása
        if (string.IsNullOrWhiteSpace(gameType))
            throw new ArgumentException("GameType nem lehet üres", nameof(gameType));

        // Játékos által választott számok mennyisége
        var count = gameType switch
        {
            "Lottery6" => 6,
            "Keno" => 10,           // Kenónál 10 számot választ a játékos
            _ => 5
        };

        var (min, max) = gameType switch
        {
            "Lottery6" => (1, 45),
            "Keno" => (1, 80),
            _ => (1, 90)
        };

        var numbers = new List<int>();
        while (numbers.Count < count)
        {
            // JAVÍTVA: Random.Shared használata (thread-safe)
            var num = Random.Shared.Next(min, max + 1);
            if (!numbers.Contains(num))
                numbers.Add(num);
        }
        return string.Join(";", numbers.OrderBy(n => n));
    }

    public string GenerateWinningNumbers(string gameType)
    {
        // JAVÍTVA: GameType validálása
        if (string.IsNullOrWhiteSpace(gameType))
            throw new ArgumentException("GameType nem lehet üres", nameof(gameType));

        // Sorsoláskor kihúzott számok mennyisége
        var count = gameType switch
        {
            "Lottery6" => 6,
            "Keno" => 20,           // Kenónál 20 számot húznak!
            _ => 5
        };

        var (min, max) = gameType switch
        {
            "Lottery6" => (1, 45),
            "Keno" => (1, 80),
            _ => (1, 90)
        };

        var numbers = new List<int>();
        while (numbers.Count < count)
        {
            // JAVÍTVA: Random.Shared használata (thread-safe)
            var num = Random.Shared.Next(min, max + 1);
            if (!numbers.Contains(num))
                numbers.Add(num);
        }
        return string.Join(";", numbers.OrderBy(n => n));
    }

    public int CalculateMatches(string ticketNumbers, string winningNumbers)
    {
        // JAVÍTVA: Null ellenőrzés
        if (string.IsNullOrWhiteSpace(ticketNumbers) || string.IsNullOrWhiteSpace(winningNumbers))
            return 0;

        try
        {
            var ticket = ticketNumbers.Split(';').Select(int.Parse).ToList();
            var winning = winningNumbers.Split(';').Select(int.Parse).ToList();
            return ticket.Intersect(winning).Count();
        }
        catch
        {
            return 0;
        }
    }

    public decimal CalculatePrize(int matches, decimal ticketPrice, string gameType, int playerNumberCount)
    {
        // JAVÍTVA: ticketPrice validálása
        if (ticketPrice <= 0)
            return 0;

        return gameType switch
        {
            "Lottery6" => matches switch
            {
                6 => ticketPrice * 5000,
                5 => ticketPrice * 50,
                4 => ticketPrice * 5,
                _ => 0
            },

            "Keno" => matches switch      // 10 számos Kenó fix
            {
                10 => ticketPrice * 10000,
                9 => ticketPrice * 2000,
                8 => ticketPrice * 500,
                7 => ticketPrice * 100,
                6 => ticketPrice * 20,
                5 => ticketPrice * 5,
                4 => ticketPrice * 2,
                0 => ticketPrice * 1,     // 0 találat = tét vissza (Keno specialitás)
                _ => 0
            },

            _ => matches switch           // 5-ös lottó
            {
                5 => ticketPrice * 10000,
                4 => ticketPrice * 100,
                3 => ticketPrice * 10,
                2 => ticketPrice * 2,
                _ => 0
            }
        };
    }

    public async Task<string> GenerateTicketCode(AppDbContext context)
    {
        const int maxRetries = 3;  // JAVÍTVA: Retry mechanizmus konkurens környezethez
        int attempt = 0;

        while (attempt < maxRetries)
        {
            try
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

                var ticketCode = $"{prefix}{sequence:D5}";

                // JAVÍTVA: Ellenőrizzük, hogy a kód még mindig unique-e (konkurens környezet)
                var exists = await context.LotteryTickets.AnyAsync(t => t.TicketCode == ticketCode);
                if (!exists)
                    return ticketCode;

                // Ha már létezik, újrapróbáljuk
                attempt++;
                await Task.Delay(10); // Kis várakozás a retry előtt
            }
            catch
            {
                attempt++;
                if (attempt >= maxRetries)
                    throw;
            }
        }

        throw new InvalidOperationException("Nem sikerült egyedi ticket kódot generálni többszöri próbálkozás után.");
    }
}