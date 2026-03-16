namespace FortunaCasino.DTOs;

public class UserDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool EmailConfirmed { get; set; }  // EZ KELL!
}