namespace MervaApi.UserTokens.Models;

public record RegisterTokenRequest(
    string Token,
    string? UserAgent,
    string? Browser,
    string? BrowserVersion,
    string? OperatingSystem,
    string? Language,
    string? Timezone,
    string? Country,
    string? Region,
    string? City,
    string? Isp,
    string? ConnectionType
);
