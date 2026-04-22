using Microsoft.EntityFrameworkCore;
using FortunaCasino.Data;
using FortunaCasino.Services.Interfaces;

namespace FortunaCasino.Services;

public class LotteryService : ILotteryService
{
    private readonly Random _random = new();

    //Játékszabályok
    private static readonly Dictionary<string, (int Min, int Max, int Pick, int? BonusMin, int? BonusMax, int BonusPick)> GameRules = new()
    {
        { "Lottery5",     (1,  90, 5, null, null, 0) },
        { "Lottery6",     (1,  45, 6, null, null, 0) },
        { "Scandinavian", (1,  35, 7, null, null, 0) },
        { "Eurojackpot",  (1,  50, 5, 1,   12,   2) },
        { "Keno",         (1,  80, 10, null, null, 0) },
        { "Joker",        (0,   9, 6, null, null, 0) },
    };

    //Nyeremény táblázatok
    private static readonly Dictionary<string, Dictionary<(int, int), decimal>> PrizeTables = new()
    {
        ["Lottery5"] = new()
        {
            { (5, 0), 250000m },
            { (4, 0), 375m   },
            { (3, 0), 25m    },
            { (2, 0), 2m     },
        },
        ["Lottery6"] = new()
        {
            { (6, 0), 500000m },
            { (5, 0), 1000m   },
            { (4, 0), 100m    },
            { (3, 0), 10m     },
        },
        ["Scandinavian"] = new()
        {
            { (7, 0), 300000m }, 
            { (6, 0), 2000m   }, 
            { (5, 0), 200m    }, 
            { (4, 0), 20m     },
            { (3, 0), 3m      },
        },
        ["Eurojackpot"] = new()
        {
            { (5, 2), 120000m },
            { (5, 1), 6000m   },
            { (5, 0), 1200m   },
            { (4, 2), 600m    },
            { (4, 1), 120m    },
            { (4, 0), 60m     },
            { (3, 2), 25m     },
            { (2, 2), 18m     },
            { (3, 1), 12m     },
            { (3, 0), 6m      },
            { (1, 2), 6m      },
            { (2, 1), 4m      },
        },
        ["Keno"] = new()
        {
            { (10, 0), 500000m },
            { (9,  0), 5000m   },
            { (8,  0), 500m    },
            { (7,  0), 50m     },
            { (6,  0), 15m     },
            { (5,  0), 4m      },
        },
        ["Joker"] = new()
        {
            { (6, 0), 500000m }, 
            { (5, 0), 5000m   },
            { (4, 0), 500m    },
            { (3, 0), 50m     },
            { (2, 0), 5m      },
            { (1, 0), 2m      },
        },
    };

    //Meglávő metódusok

    public bool ValidateNumbers(string numbers)
    {
        try
        {
            var nums = numbers.Split(';').Select(int.Parse).ToList();
            if (nums.Count != 5) return false;
            if (nums.Any(n => n < 1 || n > 90)) return false;
            if (nums.Distinct().Count() != 5) return false;
            return nums.SequenceEqual(nums.OrderBy(n => n).ToList());
        }
        catch { return false; }
    }

    public string GenerateRandomNumbers()
    {
        var numbers = new HashSet<int>();
        while (numbers.Count < 5)
            numbers.Add(_random.Next(1, 91));
        return string.Join(";", numbers.OrderBy(n => n));
    }

    public int CalculateMatches(string ticketNumbers, string winningNumbers)
    {
        try
        {
            var ticket = ticketNumbers.Split(';').Select(int.Parse).ToHashSet();
            var winning = winningNumbers.Split(';').Select(int.Parse).ToHashSet();
            return ticket.Intersect(winning).Count();
        }
        catch { return 0; }
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
        string code;
        do
        {
            var lastTicket = await context.LotteryTickets
                .Where(t => t.TicketCode.StartsWith(prefix))
                .OrderByDescending(t => t.TicketCode)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastTicket != null)
            {
                var lastSeq = lastTicket.TicketCode[9..];
                if (int.TryParse(lastSeq, out var seq))
                    sequence = seq + 1;
            }
            code = $"{prefix}{sequence:D5}";
        } while (await context.LotteryTickets.AnyAsync(t => t.TicketCode == code));

        return code;
    }

    //Újabb metódusok

    //Játéktípusonkénti szám generálás
    public string GenerateNumbersForGame(string gameType)
    {
        // 🧪 TESZT - fix nyerőszámok 
        //return "1;2;3;4;5;6;7";

        if (!GameRules.TryGetValue(gameType, out var rule))
            return GenerateRandomNumbers();

        if (gameType == "Joker")
        {
            //Joker
            var digits = Enumerable.Range(0, 6).Select(_ => _random.Next(0, 10));
            return string.Join("", digits);
        }

        var numbers = new HashSet<int>();
        while (numbers.Count < rule.Pick)
            numbers.Add(_random.Next(rule.Min, rule.Max + 1));

        var mainNumbers = string.Join(";", numbers.OrderBy(n => n));

        //Eurojackpot
        if (rule.BonusPick > 0 && rule.BonusMin.HasValue && rule.BonusMax.HasValue)
        {
            var bonus = new HashSet<int>();
            while (bonus.Count < rule.BonusPick)
                bonus.Add(_random.Next(rule.BonusMin.Value, rule.BonusMax.Value + 1));
            return $"{mainNumbers}+{string.Join(";", bonus.OrderBy(n => n))}";
        }

        return mainNumbers;
    }

    //Játéktípusonkénti validáció
    public bool ValidateNumbersForGame(string numbers, string gameType)
    {
        if (string.IsNullOrWhiteSpace(numbers)) return false;
        if (!GameRules.TryGetValue(gameType, out var rule)) return false;

        try
        {
            if (gameType == "Joker")
                return numbers.Length == 6 && numbers.All(char.IsDigit);

            // Eurojackpot: "1;2;3;4;5+1;2" formátum
            if (gameType == "Eurojackpot" && numbers.Contains('+'))
            {
                var parts = numbers.Split('+');
                var mainNums = parts[0].Split(';').Select(int.Parse).ToList();
                var bonusNums = parts[1].Split(';').Select(int.Parse).ToList();
                return mainNums.Count == rule.Pick
                    && bonusNums.Count == rule.BonusPick
                    && mainNums.All(n => n >= rule.Min && n <= rule.Max)
                    && bonusNums.All(n => n >= rule.BonusMin!.Value && n <= rule.BonusMax!.Value)
                    && mainNums.Distinct().Count() == mainNums.Count
                    && bonusNums.Distinct().Count() == bonusNums.Count;
            }

            var nums = numbers.Split(';').Select(int.Parse).ToList();
            return nums.Count == rule.Pick
                && nums.All(n => n >= rule.Min && n <= rule.Max)
                && nums.Distinct().Count() == nums.Count;
        }
        catch { return false; }
    }

    //Játéktípusonkénti találat számítás
    public int CalculateMatchesForGame(string ticketNumbers, string winningNumbers, string gameType)
    {
        try
        {
            if (gameType == "Joker")
            {
                //Joker egymás utáni egyező számjegyek az elejéről
                int count = 0;
                for (int i = 0; i < Math.Min(ticketNumbers.Length, winningNumbers.Length); i++)
                {
                    if (ticketNumbers[i] == winningNumbers[i]) count++;
                    else break;
                }
                return count;
            }

            //Eurojackpot main számok
            if (ticketNumbers.Contains('+'))
                ticketNumbers = ticketNumbers.Split('+')[0];
            if (winningNumbers.Contains('+'))
                winningNumbers = winningNumbers.Split('+')[0];

            var ticket = ticketNumbers.Split(';').Select(int.Parse).ToHashSet();
            var winning = winningNumbers.Split(';').Select(int.Parse).ToHashSet();
            return ticket.Intersect(winning).Count();
        }
        catch { return 0; }
    }

    //Bónusz találatok (Eurojackpot)
    public int CalculateBonusMatchesForGame(string ticketNumbers, string winningNumbers, string gameType)
    {
        if (gameType != "Eurojackpot") return 0;
        try
        {
            if (!ticketNumbers.Contains('+') || !winningNumbers.Contains('+')) return 0;
            var ticketBonus = ticketNumbers.Split('+')[1].Split(';').Select(int.Parse).ToHashSet();
            var winningBonus = winningNumbers.Split('+')[1].Split(';').Select(int.Parse).ToHashSet();
            return ticketBonus.Intersect(winningBonus).Count();
        }
        catch { return 0; }
    }

    //Játéktípusonkénti nyeremény számítás
    public decimal CalculatePrizeForGame(int matches, int bonusMatches, string gameType, decimal ticketPrice)
    {
        if (!PrizeTables.TryGetValue(gameType, out var table)) return 0;
        var key = (matches, bonusMatches);
        return table.TryGetValue(key, out var multiplier) ? ticketPrice * multiplier : 0;
    }
}