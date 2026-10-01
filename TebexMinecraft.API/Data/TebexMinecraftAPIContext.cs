using Microsoft.EntityFrameworkCore;
using TebexMinecraft.Modelos;

namespace TebexMinecraft.API.Data;

public class TebexMinecraftAPIContext : DbContext
{
    public TebexMinecraftAPIContext(DbContextOptions<TebexMinecraftAPIContext> options)
        : base(options)
    {
    }
    
    public DbSet<User> Users => Set<User>();
    
    public DbSet<Rank> Ranks => Set<Rank>();
    public DbSet<Prefix> Prefixes => Set<Prefix>();
    public DbSet<Consumable> Consumables => Set<Consumable>();
    public DbSet<Appeal> Appeals => Set<Appeal>();
    
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    
    public DbSet<UserRank> UserRanks => Set<UserRank>();
    public DbSet<UserPrefix> UserPrefixes => Set<UserPrefix>();
    public DbSet<UserClaim> UserClaims => Set<UserClaim>();
    public DbSet<UserAppeal> UserAppeals => Set<UserAppeal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Uuid)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username);
        
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderCode)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<UserRank>()
            .HasIndex(ur => new { ur.Uuid, ur.ServerRank })
            .IsUnique();
        
        modelBuilder.Entity<UserPrefix>()
            .HasIndex(up => new { up.Uuid, up.ServerTag })
            .IsUnique();
        
        modelBuilder.Entity<UserClaim>()
            .HasIndex(uc => new { uc.Uuid, uc.IsClaimed });
        
        modelBuilder.Entity<UserAppeal>()
            .HasIndex(ua => new { ua.Uuid, ua.IsProcessed });
    }
}