using MervaApi.Data;
using MervaApi.UserPreferences.Models;
using Microsoft.EntityFrameworkCore;

namespace MervaApi.UserPreferences.Services;

public class UserPreferenceService(MervaDbContext db) : IUserPreferenceService
{
    public async Task UpsertFavoriteCurrencyAsync(int tokenId, string currency)
    {
        var preference = await db.UserPreferences
            .FirstOrDefaultAsync(p => p.TokenId == tokenId);

        if (preference is null)
        {
            db.UserPreferences.Add(new UserPreference
            {
                TokenId         = tokenId,
                DefaultCurrency = currency,
                UpdatedAt       = DateTime.UtcNow,
            });
        }
        else
        {
            preference.DefaultCurrency = currency;
            preference.UpdatedAt       = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    public async Task<string?> GetDefaultCurrencyAsync(int tokenId)
    {
        var preference = await db.UserPreferences
            .FirstOrDefaultAsync(p => p.TokenId == tokenId);

        return preference?.DefaultCurrency;
    }
}
