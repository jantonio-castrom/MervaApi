using MervaApi.Data;
using MervaApi.Encryption.Services;
using Xunit;
using MervaApi.UserTokens.Models;
using MervaApi.UserTokens.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MervaTests.UserTokens;

public class UserTokenServiceTests
{
    private static (MervaDbContext Db, EncryptionService Enc) CreateDeps()
    {
        var options = new DbContextOptionsBuilder<MervaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                { "USERTOKEN_KEY", Convert.ToBase64String(new byte[32]) }
            })
            .Build();
        return (new MervaDbContext(options), new EncryptionService(config));
    }

    private static RegisterTokenRequest Req(string token, string browser = "Chrome") =>
        new(token, "Mozilla/5.0", browser, "120", "Windows", "en-US", "UTC", "US", "CA", "LA", "Comcast", "broadband");

    [Fact]
    public async Task RegisterAsync_NewToken_ReturnsIsNewTrue()
    {
        var (db, enc) = CreateDeps();
        var svc = new UserTokenService(db, enc);

        var (_, isNew) = await svc.RegisterAsync(Req("abc"), "1.2.3.4");

        Assert.True(isNew);
    }

    [Fact]
    public async Task RegisterAsync_NewToken_CreatesTokenAndDevice()
    {
        var (db, enc) = CreateDeps();
        var svc = new UserTokenService(db, enc);

        await svc.RegisterAsync(Req("abc"), "1.2.3.4");

        Assert.Equal(1, await db.UserTokens.CountAsync());
        Assert.Equal(1, await db.UserDevices.CountAsync());
    }

    [Fact]
    public async Task RegisterAsync_ExistingToken_SameDevice_ReturnsIsNewFalse()
    {
        var (db, enc) = CreateDeps();
        var svc = new UserTokenService(db, enc);
        await svc.RegisterAsync(Req("abc"), "1.2.3.4");

        var (_, isNew) = await svc.RegisterAsync(Req("abc"), "1.2.3.4");

        Assert.False(isNew);
    }

    [Fact]
    public async Task RegisterAsync_ExistingToken_SameDevice_DoesNotAddNewDevice()
    {
        var (db, enc) = CreateDeps();
        var svc = new UserTokenService(db, enc);
        await svc.RegisterAsync(Req("abc"), "1.2.3.4");

        await svc.RegisterAsync(Req("abc"), "1.2.3.4");

        Assert.Equal(1, await db.UserDevices.CountAsync());
    }

    [Fact]
    public async Task RegisterAsync_ExistingToken_DifferentBrowser_AddsNewDevice()
    {
        var (db, enc) = CreateDeps();
        var svc = new UserTokenService(db, enc);
        await svc.RegisterAsync(Req("abc", "Chrome"), "1.2.3.4");

        await svc.RegisterAsync(Req("abc", "Firefox"), "1.2.3.4");

        Assert.Equal(2, await db.UserDevices.CountAsync());
    }

    [Fact]
    public async Task RegisterAsync_ExistingToken_DifferentIp_AddsNewDevice()
    {
        var (db, enc) = CreateDeps();
        var svc = new UserTokenService(db, enc);
        await svc.RegisterAsync(Req("abc"), "1.2.3.4");

        await svc.RegisterAsync(Req("abc"), "9.9.9.9");

        Assert.Equal(2, await db.UserDevices.CountAsync());
    }

    [Fact]
    public async Task TokenExistsAsync_RegisteredToken_ReturnsTrue()
    {
        var (db, enc) = CreateDeps();
        var svc = new UserTokenService(db, enc);
        await svc.RegisterAsync(Req("known"), null);

        Assert.True(await svc.TokenExistsAsync("known"));
    }

    [Fact]
    public async Task TokenExistsAsync_UnknownToken_ReturnsFalse()
    {
        var (db, enc) = CreateDeps();
        var svc = new UserTokenService(db, enc);

        Assert.False(await svc.TokenExistsAsync("ghost"));
    }

    [Fact]
    public async Task GetByTokenAsync_KnownToken_ReturnsDecryptedToken()
    {
        var (db, enc) = CreateDeps();
        var svc = new UserTokenService(db, enc);
        await svc.RegisterAsync(Req("my-token"), null);

        var result = await svc.GetByTokenAsync("my-token");

        Assert.NotNull(result);
        Assert.Equal("my-token", result.Value.Token);
    }

    [Fact]
    public async Task GetByTokenAsync_UnknownToken_ReturnsNull()
    {
        var (db, enc) = CreateDeps();
        var svc = new UserTokenService(db, enc);

        Assert.Null(await svc.GetByTokenAsync("nobody"));
    }

    [Fact]
    public async Task RegisterAsync_MultipleDistinctTokens_AllStored()
    {
        var (db, enc) = CreateDeps();
        var svc = new UserTokenService(db, enc);

        await svc.RegisterAsync(Req("token-a"), null);
        await svc.RegisterAsync(Req("token-b"), null);
        await svc.RegisterAsync(Req("token-c"), null);

        Assert.Equal(3, await db.UserTokens.CountAsync());
    }
}
