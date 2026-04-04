namespace FortunaCasino.DTOs.Lottery;

public class PurchaseRequest
{
    public List<CartTicketItem> Tickets { get; set; } = new();
}

public class CartTicketItem
{
    public int GameId { get; set; }
    public string GameName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public object? Numbers { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; } = 1;
}