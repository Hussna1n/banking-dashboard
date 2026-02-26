namespace BankingAPI.Models;

public class User
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public ICollection<Account> Accounts { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Account
{
    public int Id { get; set; }
    public required string AccountNumber { get; set; }
    public string Type { get; set; } = "checking"; // checking|savings|investment
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;
    public int UserId { get; set; }
    public User? User { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Transaction
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = "debit"; // debit|credit|transfer
    public required string Description { get; set; }
    public string? Category { get; set; }
    public string? Reference { get; set; }
    public decimal BalanceAfter { get; set; }
    public int AccountId { get; set; }
    public Account? Account { get; set; }
    public int? ToAccountId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Beneficiary
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string AccountNumber { get; set; }
    public string? BankName { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
}
