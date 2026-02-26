using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BankingAPI.Data;
using BankingAPI.Models;
using System.Security.Claims;

namespace BankingAPI.Controllers;

[ApiController, Route("api/accounts"), Authorize]
public class AccountsController(AppDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var accounts = await db.Accounts
            .Where(a => a.UserId == UserId && a.IsActive)
            .Include(a => a.Transactions.OrderByDescending(t => t.CreatedAt).Take(5))
            .ToListAsync();
        return Ok(accounts);
    }

    [HttpGet("{id}/transactions")]
    public async Task<IActionResult> GetTransactions(int id,
        [FromQuery] int page = 1, [FromQuery] int limit = 20,
        [FromQuery] string? type, [FromQuery] string? category)
    {
        var query = db.Transactions.Where(t => t.AccountId == id);
        if (!string.IsNullOrEmpty(type)) query = query.Where(t => t.Type == type);
        if (!string.IsNullOrEmpty(category)) query = query.Where(t => t.Category == category);

        var total = await query.CountAsync();
        var txns = await query.OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * limit).Take(limit).ToListAsync();
        return Ok(new { transactions = txns, total, pages = (int)Math.Ceiling(total / (double)limit) });
    }

    [HttpPost("{id}/transfer")]
    public async Task<IActionResult> Transfer(int id, [FromBody] TransferRequest req)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        var fromAcc = await db.Accounts.FindAsync(id);
        var toAcc = await db.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == req.ToAccountNumber);

        if (fromAcc is null || toAcc is null) return NotFound("Account not found");
        if (fromAcc.UserId != UserId) return Forbid();
        if (fromAcc.Balance < req.Amount) return BadRequest("Insufficient funds");

        fromAcc.Balance -= req.Amount;
        toAcc.Balance += req.Amount;

        db.Transactions.Add(new Transaction { AccountId = id, Type = "debit", Amount = req.Amount, Description = req.Description ?? $"Transfer to {req.ToAccountNumber}", BalanceAfter = fromAcc.Balance, ToAccountId = toAcc.Id });
        db.Transactions.Add(new Transaction { AccountId = toAcc.Id, Type = "credit", Amount = req.Amount, Description = $"Transfer from {fromAcc.AccountNumber}", BalanceAfter = toAcc.Balance, ToAccountId = id });

        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(new { newBalance = fromAcc.Balance });
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> Analytics()
    {
        var accounts = await db.Accounts.Where(a => a.UserId == UserId).Select(a => a.Id).ToListAsync();
        var now = DateTime.UtcNow;
        var start = new DateTime(now.Year, now.Month, 1);

        var monthly = await db.Transactions
            .Where(t => accounts.Contains(t.AccountId) && t.CreatedAt >= now.AddMonths(-5))
            .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month, t.Type })
            .Select(g => new { g.Key.Year, g.Key.Month, g.Key.Type, Total = g.Sum(t => t.Amount) })
            .ToListAsync();

        var byCategory = await db.Transactions
            .Where(t => accounts.Contains(t.AccountId) && t.CreatedAt >= start)
            .GroupBy(t => t.Category)
            .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
            .ToListAsync();

        var totalBalance = await db.Accounts.Where(a => a.UserId == UserId && a.IsActive).SumAsync(a => a.Balance);

        return Ok(new { monthly, byCategory, totalBalance });
    }
}

public record TransferRequest(string ToAccountNumber, decimal Amount, string? Description);
