using FortunaCasino.Data;

namespace FortunaCasino.Services.Interfaces;

public interface ILotteryService
{
    bool ValidateNumbers(string numbers, string gameType);
    string GenerateRandomNumbers(string gameType);        // Játékos számai
    string GenerateWinningNumbers(string gameType);       // Nyerőszámok (Keno: 20!)
    int CalculateMatches(string ticketNumbers, string winningNumbers);
    decimal CalculatePrize(int matches, decimal ticketPrice, string gameType, int playerNumberCount);
    Task<string> GenerateTicketCode(AppDbContext context);
}