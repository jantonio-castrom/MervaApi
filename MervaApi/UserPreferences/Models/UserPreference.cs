using MervaApi.UserTokens.Models;

namespace MervaApi.UserPreferences.Models;

public class UserPreference
{
    public int PreferenceId { get; set; }
    public int TokenId { get; set; }
    public string DefaultCurrency { get; set; } = "USD";
    public string Theme { get; set; } = "light";
    public DateTime UpdatedAt { get; set; }

    public UserToken UserToken { get; set; } = null!;
}
