using Microsoft.EntityFrameworkCore;
using BankingAPI.Models;

namespace BankingAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Account>()
            .Property(a => a.Balance)
            .HasPrecision(18, 2);

        b.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasPrecision(18, 2);

        b.Entity<Transaction>()
            .HasOne(t => t.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Account>()
            .HasIndex(a => a.AccountNumber)
            .IsUnique();
    }
}
