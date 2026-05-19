using MervaApi.UserTokens.Models;

namespace MervaApi.UserTokens.Services;

public interface IUserTokenService
{
    Task<bool> TokenExistsAsync(string token);
    Task<(UserToken Token, bool IsNew)> RegisterAsync(RegisterTokenRequest request, string? ipAddress);
    Task<(int TokenId, string Token, bool IsPremium)?> GetByTokenAsync(string token);
    Task<bool> GetIsPremiumAsync(int tokenId);
    Task<int> CountTransactionsAsync(int tokenId);
}
