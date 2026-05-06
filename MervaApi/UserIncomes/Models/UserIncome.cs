using MervaApi.UserTokens.Models;

namespace MervaApi.UserIncomes.Models;

public class UserIncome
{
    public int IncomeId { get; set; }
    public int TokenId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Amount { get; set; } = "0";
    public string Currency { get; set; } = "USD";
    public string? Category { get; set; }
    public DateOnly IncomeDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public UserToken UserToken { get; set; } = null!;
}
