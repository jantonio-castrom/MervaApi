using MervaApi.Data;
using MervaApi.Encryption.Services;
using MervaApi.UserIncomes.Models;
using Microsoft.EntityFrameworkCore;

namespace MervaApi.UserIncomes.Services;

public class UserIncomeService(MervaDbContext db, IEncryptionService encryptionService) : IUserIncomeService
{
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
}
