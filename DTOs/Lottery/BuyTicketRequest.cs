using System.ComponentModel.DataAnnotations;

namespace FortunaCasino.DTOs.Lottery;

public class BuyTicketRequest
{
    [Required]
    public long DrawId { get; set; }

    public string? FieldA { get; set; }
    public string? FieldB { get; set; }
    public string? FieldC { get; set; }
    public string? FieldD { get; set; }
    public string? FieldE { get; set; }
    public string? FieldF { get; set; }

    public bool IsQuickPick { get; set; }
}