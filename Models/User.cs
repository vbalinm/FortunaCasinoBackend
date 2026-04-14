using System.ComponentModel.DataAnnotations;

namespace FortunaCasino.Models;

public class User
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public decimal Balance { get; set; } = 100.00m;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool EmailConfirmed { get; set; } = false;
    public string? EmailConfirmationToken { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetExpires { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public List<string> Roles => UserRoles?.Select(ur => ur.Role?.RoleName ?? string.Empty).Where(r => !string.IsNullOrEmpty(r)).ToList() ?? new List<string>();
}