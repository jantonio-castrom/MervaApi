using MervaApi.Data;
using MervaApi.Encryption.Services;
using Xunit;
using MervaApi.UserExpenses.Models;
using MervaApi.UserExpenses.Services;
using MervaApi.UserPreferences.Services;
using MervaApi.UserTokens.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MervaTests.UserExpenses;

public class UserExpenseServiceTests
{
    private sealed class StubPreferenceService : IUserPreferenceService
    {
        public Task UpsertFavoriteCurrencyAsync(int tokenId, string currency) => Task.CompletedTask;
        public Task<string?> GetDefaultCurrencyAsync(int tokenId) => Task.FromResult<string?>(null);
    }

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

    private static UserExpenseService CreateService(MervaDbContext db, EncryptionService enc) =>
        new(db, enc, new StubPreferenceService());

    private static UserToken SeedToken(MervaDbContext db, EncryptionService enc, string rawToken)
    {
        var token = new UserToken
        {
            Token = enc.Encrypt(rawToken),
            EncryptedValueHash = enc.ComputeSha256(rawToken),
        };
        db.UserTokens.Add(token);
        db.SaveChanges();
        return token;
    }

    private static AddExpenseRequest MakeRequest(string token, string name, decimal amount, DateOnly date) =>
        new(token, name, amount, "USD", "Other", date, null);

    [Fact]
    public async Task AddExpenseAsync_ValidToken_PersistsExpense()
    {
        var (db, enc) = CreateDeps();
        SeedToken(db, enc, "tok");
        var svc = CreateService(db, enc);

        var result = await svc.AddExpenseAsync(MakeRequest("tok", "Coffee", 4.50m, new DateOnly(2025, 1, 1)));

        Assert.NotNull(result);
        Assert.Equal(1, await db.Expenses.CountAsync());
    }

    [Fact]
    public async Task AddExpenseAsync_UnknownToken_ReturnsNull()
    {
        var (db, enc) = CreateDeps();
        var svc = CreateService(db, enc);

        var result = await svc.AddExpenseAsync(MakeRequest("ghost", "Coffee", 4.50m, new DateOnly(2025, 1, 1)));

        Assert.Null(result);
        Assert.Equal(0, await db.Expenses.CountAsync());
    }

    [Fact]
    public async Task AddExpenseAsync_EncryptsStoredFields()
    {
        var (db, enc) = CreateDeps();
        SeedToken(db, enc, "tok");
        var svc = CreateService(db, enc);

        await svc.AddExpenseAsync(MakeRequest("tok", "Lunch", 12m, new DateOnly(2025, 1, 1)));

        var stored = await db.Expenses.FirstAsync();
        Assert.NotEqual("Lunch", stored.Name);
        Assert.NotEqual("12", stored.Amount);
    }

    [Fact]
    public async Task GetExpensesAsync_ReturnsDecryptedValues()
    {
        var (db, enc) = CreateDeps();
        var token = SeedToken(db, enc, "tok");
        db.Expenses.Add(new Expense
        {
            TokenId = token.TokenId,
            Name = enc.Encrypt("Groceries"),
            Amount = enc.Encrypt("55.20"),
            Currency = enc.Encrypt("EUR"),
            Category = enc.Encrypt("Food & Dining"),
            ExpenseDate = new DateOnly(2025, 3, 1),
        });
        db.SaveChanges();
        var svc = CreateService(db, enc);

        var results = await svc.GetExpensesAsync(token.TokenId);

        Assert.Single(results);
        Assert.Equal("Groceries", results[0].Name);
        Assert.Equal(55.20m, results[0].Amount);
        Assert.Equal("EUR", results[0].Currency);
        Assert.Equal("Food & Dining", results[0].Category);
    }

    [Fact]
    public async Task GetExpensesAsync_ReturnsOrderedByDateDescending()
    {
        var (db, enc) = CreateDeps();
        var token = SeedToken(db, enc, "tok");
        var svc = CreateService(db, enc);

        await svc.AddExpenseAsync(MakeRequest("tok", "Old", 1m, new DateOnly(2024, 1, 1)));
        await svc.AddExpenseAsync(MakeRequest("tok", "New", 2m, new DateOnly(2025, 6, 1)));
        await svc.AddExpenseAsync(MakeRequest("tok", "Mid", 3m, new DateOnly(2024, 12, 1)));

        var results = await svc.GetExpensesAsync(token.TokenId);

        Assert.Equal("New", results[0].Name);
        Assert.Equal("Mid", results[1].Name);
        Assert.Equal("Old", results[2].Name);
    }

    [Fact]
    public async Task GetExpensesAsync_NullCategory_ReturnsNullInResponse()
    {
        var (db, enc) = CreateDeps();
        var token = SeedToken(db, enc, "tok");
        db.Expenses.Add(new Expense
        {
            TokenId = token.TokenId,
            Name = enc.Encrypt("Item"),
            Amount = enc.Encrypt("10"),
            Currency = enc.Encrypt("USD"),
            Category = null,
            ExpenseDate = new DateOnly(2025, 1, 1),
        });
        db.SaveChanges();
        var svc = CreateService(db, enc);

        var results = await svc.GetExpensesAsync(token.TokenId);

        Assert.Null(results[0].Category);
    }

    [Fact]
    public async Task GetExpensesAsync_WrongTokenId_ReturnsEmpty()
    {
        var (db, enc) = CreateDeps();
        var token = SeedToken(db, enc, "tok");
        var svc = CreateService(db, enc);
        await svc.AddExpenseAsync(MakeRequest("tok", "Coffee", 4m, new DateOnly(2025, 1, 1)));

        var results = await svc.GetExpensesAsync(token.TokenId + 99);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetExpensesAsync_WithFromDate_ExcludesOlderExpenses()
    {
        var (db, enc) = CreateDeps();
        var token = SeedToken(db, enc, "tok");
        var svc = CreateService(db, enc);
        var fromDate = new DateOnly(2025, 3, 1);

        await svc.AddExpenseAsync(MakeRequest("tok", "Recent", 10m, new DateOnly(2025, 4, 1)));
        await svc.AddExpenseAsync(MakeRequest("tok", "Old", 5m, new DateOnly(2025, 2, 28)));

        var results = await svc.GetExpensesAsync(token.TokenId, fromDate);

        Assert.Single(results);
        Assert.Equal("Recent", results[0].Name);
    }

    [Fact]
    public async Task GetExpensesAsync_WithFromDate_IncludesExpenseOnBoundaryDate()
    {
        var (db, enc) = CreateDeps();
        var token = SeedToken(db, enc, "tok");
        var svc = CreateService(db, enc);
        var fromDate = new DateOnly(2025, 3, 1);

        await svc.AddExpenseAsync(MakeRequest("tok", "Boundary", 7m, fromDate));

        var results = await svc.GetExpensesAsync(token.TokenId, fromDate);

        Assert.Single(results);
        Assert.Equal("Boundary", results[0].Name);
    }

    [Fact]
    public async Task GetExpensesAsync_WithNullFromDate_ReturnsAllExpenses()
    {
        var (db, enc) = CreateDeps();
        var token = SeedToken(db, enc, "tok");
        var svc = CreateService(db, enc);

        await svc.AddExpenseAsync(MakeRequest("tok", "VeryOld", 1m, new DateOnly(2020, 1, 1)));
        await svc.AddExpenseAsync(MakeRequest("tok", "Recent", 2m, new DateOnly(2025, 5, 1)));

        var results = await svc.GetExpensesAsync(token.TokenId, fromDate: null);

        Assert.Equal(2, results.Count);
    }
}
