using MervaApi.Data;
using MervaApi.Encryption.Services;
using MervaApi.UserExpenses.Models;
using MervaApi.UserPreferences.Services;
using Microsoft.EntityFrameworkCore;

namespace MervaApi.UserExpenses.Services;

public class UserExpenseService(
    MervaDbContext db,
    IEncryptionService encryptionService,
    IUserPreferenceService preferenceService) : IUserExpenseService
{
    public async Task<Expense?> AddExpenseAsync(AddExpenseRequest request)
    {
        var hashedToken = encryptionService.ComputeSha256(request.Token);
        var tokenId = await db.UserTokens
            .AsNoTracking()
            .Where(t => t.EncryptedValueHash == hashedToken)
            .Select(t => (int?)t.TokenId)
            .FirstOrDefaultAsync();

        if (tokenId is null)
            return null;

        var expense = new Expense
        {
            TokenId     = tokenId.Value,
            Name        = encryptionService.Encrypt(request.Name ?? "Default"),
            Amount      = encryptionService.Encrypt(request.Amount.ToString() ?? "0"),
            Currency    = encryptionService.Encrypt(request.Currency ?? "USD"),
            Category    = encryptionService.Encrypt(request.Category ?? "Other"),
            ExpenseDate = request.ExpenseDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
        };

        db.Expenses.Add(expense);
        await db.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(request.FavoriteCurrency))
            await preferenceService.UpsertFavoriteCurrencyAsync(tokenId.Value, request.FavoriteCurrency);

        return expense;
    }

    public async Task<IReadOnlyList<ExpenseResponse>> GetExpensesAsync(int tokenId)
    {
        var expenses = await db.Expenses
            .AsNoTracking()
            .Where(e => e.TokenId == tokenId)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();

        return expenses.Select(e => new ExpenseResponse(
            e.ExpenseId,
            encryptionService.Decrypt(e.Name),
            decimal.TryParse(encryptionService.Decrypt(e.Amount), out var amount) ? amount : 0,
            encryptionService.Decrypt(e.Currency),
            e.Category is null ? null : encryptionService.Decrypt(e.Category),
            e.ExpenseDate,
            e.CreatedAt
        )).ToList();
    }

    public async Task<bool> SoftDeleteExpenseAsync(int expenseId, int tokenId)
    {
        var expense = await db.Expenses
            .FirstOrDefaultAsync(e => e.ExpenseId == expenseId && e.TokenId == tokenId);

        if (expense is null)
            return false;

        expense.IsDeleted = true;
        expense.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }
}
