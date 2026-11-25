using GameApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GameApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Character> Characters => Set<Character>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Accounts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Username).IsRequired().HasMaxLength(50);
            entity.HasIndex(x => x.Username).IsUnique();
            entity.Property(x => x.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(x => x.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Character>(entity =>
        {
            entity.ToTable("Characters");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Class).IsRequired().HasMaxLength(30);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasOne(x => x.Account)
                  .WithMany(x => x.Characters)
                  .HasForeignKey(x => x.AccountId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
