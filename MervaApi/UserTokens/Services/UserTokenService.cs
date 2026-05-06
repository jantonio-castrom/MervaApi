using MervaApi.Data;
using MervaApi.Encryption.Services;
using MervaApi.UserTokens.Models;
using Microsoft.EntityFrameworkCore;

namespace MervaApi.UserTokens.Services;

public class UserTokenService(MervaDbContext db, IEncryptionService encryptionService) : IUserTokenService
{
    public Task<bool> TokenExistsAsync(string token) =>
        db.UserTokens.AnyAsync(t => t.Token == token);

    public async Task<(UserToken Token, bool IsNew)> RegisterAsync(RegisterTokenRequest request, string? ipAddress)
    {
        var hashedValue = encryptionService.ComputeSha256(request.Token);
        var existing = await db.UserTokens.FirstOrDefaultAsync(t => t.EncryptedValueHash == hashedValue);

        UserToken userToken;
        bool isNew;

        if (existing is not null)
        {
            userToken = existing;
            isNew = false;
        }
        else
        {
            userToken = new UserToken
            {
                Token              = encryptionService.Encrypt(request.Token),
                EncryptedValueHash = hashedValue,
            };
            db.UserTokens.Add(userToken);
            await db.SaveChangesAsync();
            isNew = true;
        }

        if (isNew || await HasDeviceChangedAsync(userToken.TokenId, request, ipAddress))
        {
            db.UserDevices.Add(new UserDevice
            {
                TokenId         = userToken.TokenId,
                UserAgent       = request.UserAgent,
                Browser         = request.Browser,
                BrowserVersion  = request.BrowserVersion,
                OperatingSystem = request.OperatingSystem,
                Language        = request.Language,
                Timezone        = request.Timezone,
                IpAddress       = ipAddress,
                Country         = request.Country,
                Region          = request.Region,
                City            = request.City,
                Isp             = request.Isp,
                ConnectionType  = request.ConnectionType,
            });
            await db.SaveChangesAsync();
        }

        return (userToken, isNew);
    }

    private async Task<bool> HasDeviceChangedAsync(int tokenId, RegisterTokenRequest request, string? ipAddress)
    {
        var latest = await db.UserDevices
            .AsNoTracking()
            .Where(d => d.TokenId == tokenId)
            .OrderByDescending(d => d.RecordedAt)
            .Select(d => new
            {
                d.Browser,
                d.BrowserVersion,
                d.OperatingSystem,
                d.Language,
                d.Timezone,
                d.IpAddress,
                d.Country,
                d.Region,
                d.City,
                d.Isp,
                d.ConnectionType,
            })
            .FirstOrDefaultAsync();

        if (latest is null)
            return true;

        return latest.Browser         != request.Browser
            || latest.BrowserVersion  != request.BrowserVersion
            || latest.OperatingSystem != request.OperatingSystem
            || latest.Language        != request.Language
            || latest.Timezone        != request.Timezone
            || latest.IpAddress       != ipAddress
            || latest.Country         != request.Country
            || latest.Region          != request.Region
            || latest.City            != request.City
            || latest.Isp             != request.Isp
            || latest.ConnectionType  != request.ConnectionType;
    }

    public async Task<(int TokenId, string Token)?> GetByTokenAsync(string token)
    {
        var hashedValue = encryptionService.ComputeSha256(token);
        var result = await db.UserTokens
            .AsNoTracking()
            .Where(t => t.EncryptedValueHash == hashedValue)
            .Select(t => new { t.TokenId, t.Token })
            .FirstOrDefaultAsync();

        if (result is null)
            return null;

        return (result.TokenId, encryptionService.Decrypt(result.Token));
    }
}
