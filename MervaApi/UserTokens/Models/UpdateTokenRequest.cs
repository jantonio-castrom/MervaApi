namespace MervaApi.UserTokens.Models;

public record UpdateTokenRequest(
    string Token,
    string? UserAgent,
    string? Browser,
    string? BrowserVersion,
    string? OperatingSystem,
    string? Language,
    string? Timezone,
    string? ConnectionType
);
