using MervaApi.UserExpenses.Models;
using MervaApi.UserIncomes.Models;
using MervaApi.UserPreferences.Models;

namespace MervaApi.UserTokens.Models;

public class UserToken
{
    public int TokenId { get; set; }
    public string Token { get; set; } = string.Empty;
    public byte[] EncryptedValueHash { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public bool IsPremium { get; set; }

    public ICollection<Expense> Expenses { get; set; } = [];
    public ICollection<UserIncome> Incomes { get; set; } = [];
    public ICollection<UserDevice> Devices { get; set; } = [];
    public UserPreference? Preference { get; set; }
}
