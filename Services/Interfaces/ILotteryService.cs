using FortunaCasino.Data;

namespace FortunaCasino.Services.Interfaces;

public interface ILotteryService
{
    bool ValidateNumbers(string numbers);
    string GenerateRandomNumbers();
    int CalculateMatches(string ticketNumbers, string winningNumbers);
    decimal CalculatePrize(int matches, decimal ticketPrice);
    Task<string> GenerateTicketCode(AppDbContext context);

    string GenerateNumbersForGame(string gameType);
    bool ValidateNumbersForGame(string numbers, string gameType);
    int CalculateMatchesForGame(string ticketNumbers, string winningNumbers, string gameType);
    decimal CalculatePrizeForGame(int matches, int bonusMatches, string gameType, decimal ticketPrice);
}