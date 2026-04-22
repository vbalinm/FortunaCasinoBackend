namespace FortunaCasino.Models;

public class UserRole
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public byte RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public DateTime AssignedAt { get; set; } = DateTime.Now;
}