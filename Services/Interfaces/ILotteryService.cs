using FortunaCasino.Data;

namespace FortunaCasino.Services.Interfaces;

public interface ILotteryService
{
    bool ValidateNumbers(string numbers);
    string GenerateRandomNumbers();
    int CalculateMatches(string ticketNumbers, string winningNumbers);
    decimal CalculatePrize(int matches, decimal ticketPrice);
    Task<string> GenerateTicketCode(AppDbContext context);
}