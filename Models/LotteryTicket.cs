namespace FortunaCasino.Models;

public class LotteryTicket
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public long DrawId { get; set; }
    public LotteryDraw Draw { get; set; } = null!;
    public string TicketCode { get; set; } = string.Empty;
    public string? FieldsNumbers { get; set; }
    public int Fields { get; set; } = 1;
    public byte FieldsFilled { get; set; } = 1;

    public bool IsQuickPick { get; set; }
    public decimal TotalPrice { get; set; }
    public byte? MatchesNumbers { get; set; }
    public byte? Matches { get; set; }
    public decimal TotalWinAmount { get; set; }
    public bool IsPaidOut { get; set; }
    public string Status { get; set; } = "active";
    public DateTime BoughtAt { get; set; } = DateTime.UtcNow;
}