namespace FortunaCasino.DTOs.Lottery;

public class TicketResponse
{
    public long Id { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public DateTime DrawDate { get; set; }
    public string? FieldA { get; set; }
    public string? FieldB { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal? WinAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}