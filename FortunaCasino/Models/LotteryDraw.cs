namespace FortunaCasino.Models;

public class LotteryDraw
{
    public long Id { get; set; }
    public DateTime DrawDate { get; set; }
    public decimal TicketPrice { get; set; } = 400.00m;
    public string? WinningNumbers { get; set; }
    public bool IsDrawn { get; set; }
    public bool IsActive { get; set; } = true;
    public int TotalTicketsSold { get; set; }
    public decimal TotalPayout { get; set; }
    public DateTime? DrawnAt { get; set; }
    public long? DrawnBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string GameType { get; set; } = "Lottery5";
    public ICollection<LotteryTicket> Tickets { get; set; } = new List<LotteryTicket>();
}