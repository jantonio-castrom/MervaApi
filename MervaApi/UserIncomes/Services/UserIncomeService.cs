using MervaApi.Data;
using MervaApi.Encryption.Services;
using MervaApi.UserIncomes.Models;
using Microsoft.EntityFrameworkCore;

namespace MervaApi.UserIncomes.Services;

public class UserIncomeService(MervaDbContext db, IEncryptionService encryptionService) : IUserIncomeService
{
    public async Task<UserIncome?> AddIncomeAsync(AddIncomeRequest request)
    {
        var hashedToken = encryptionService.ComputeSha256(request.Token);
        var tokenId = await db.UserTokens
            .AsNoTracking()
            .Where(t => t.EncryptedValueHash == hashedToken)
            .Select(t => (int?)t.TokenId)
            .FirstOrDefaultAsync();

        if (tokenId is null)
            return null;

        var income = new UserIncome
        {
            TokenId    = tokenId.Value,
            Name       = encryptionService.Encrypt(request.Name ?? "Default"),
            Amount     = encryptionService.Encrypt(request.Amount.ToString() ?? "0"),
            Currency   = encryptionService.Encrypt(request.Currency ?? "USD"),
            Category   = encryptionService.Encrypt(request.Category ?? "Other"),
            IncomeDate = request.IncomeDate,
        };

        db.UserIncomes.Add(income);
        await db.SaveChangesAsync();
        return income;
    }

    public async Task<IReadOnlyList<IncomeResponse>> GetIncomesAsync(int tokenId)
    {
        var incomes = await db.UserIncomes
            .AsNoTracking()
            .Where(i => i.TokenId == tokenId)
            .OrderByDescending(i => i.IncomeDate)
            .ToListAsync();

        return incomes.Select(i => new IncomeResponse(
            i.IncomeId,
            encryptionService.Decrypt(i.Name),
            decimal.TryParse(encryptionService.Decrypt(i.Amount), out var amount) ? amount : 0,
            encryptionService.Decrypt(i.Currency),
            i.Category is null ? null : encryptionService.Decrypt(i.Category),
            i.IncomeDate,
            i.CreatedAt
        )).ToList();
    }

    public async Task<bool> SoftDeleteIncomeAsync(int incomeId, int tokenId)
    {
        var income = await db.UserIncomes
            .FirstOrDefaultAsync(i => i.IncomeId == incomeId && i.TokenId == tokenId);

        if (income is null)
            return false;

        income.IsDeleted = true;
        income.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }
}
