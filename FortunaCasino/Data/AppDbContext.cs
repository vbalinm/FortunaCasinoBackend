using Microsoft.EntityFrameworkCore;
using FortunaCasino.Models;

namespace FortunaCasino.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<LotteryDraw> LotteryDraws => Set<LotteryDraw>();
    public DbSet<LotteryTicket> LotteryTickets => Set<LotteryTicket>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Összetett kulcs
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        // Egyedi index a TicketCode-ra
        modelBuilder.Entity<LotteryTicket>()
            .HasIndex(t => t.TicketCode)
            .IsUnique();

        // Alapadatok
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, RoleName = "user" },
            new Role { Id = 2, RoleName = "admin" }
        );
    }
}