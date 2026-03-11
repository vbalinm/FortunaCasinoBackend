namespace FortunaCasino.Models;

public class LotteryTicket
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public long DrawId { get; set; }
    public LotteryDraw Draw { get; set; } = null!;
    public string TicketCode { get; set; } = string.Empty; // LOT format

    public string? FieldA { get; set; }
    public string? FieldB { get; set; }
    public string? FieldC { get; set; }
    public string? FieldD { get; set; }
    public string? FieldE { get; set; }
    public string? FieldF { get; set; }

    public byte FieldsFilled { get; set; } = 1;
    public bool IsQuickPick { get; set; }
    public decimal TotalPrice { get; set; }

    public byte? MatchesA { get; set; }
    public byte? MatchesB { get; set; }
    public byte? MatchesC { get; set; }
    public byte? MatchesD { get; set; }
    public byte? MatchesE { get; set; }
    public byte? MatchesF { get; set; }

    public decimal TotalWinAmount { get; set; }
    public bool IsPaidOut { get; set; }
    public string Status { get; set; } = "active";
    public DateTime BoughtAt { get; set; } = DateTime.UtcNow;
}