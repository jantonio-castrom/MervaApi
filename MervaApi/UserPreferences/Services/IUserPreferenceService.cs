namespace MervaApi.UserPreferences.Services;

public interface IUserPreferenceService
{
    Task UpsertFavoriteCurrencyAsync(int tokenId, string currency);
    Task<string?> GetDefaultCurrencyAsync(int tokenId);
}
