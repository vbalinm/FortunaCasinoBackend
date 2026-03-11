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

    // JAVÍTVA: Inicializálás üres listával, hogy sose legyen null
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    // JAVÍTVA: Biztonságosabb Roles property null-ellenőrzéssel
    public List<string> Roles
    {
        get
        {
            if (UserRoles == null)
                return new List<string>();

            return UserRoles
                .Where(ur => ur != null && ur.Role != null)
                .Select(ur => ur.Role!.RoleName)
                .Where(r => !string.IsNullOrEmpty(r))
                .ToList() ?? new List<string>();
        }
    }
}