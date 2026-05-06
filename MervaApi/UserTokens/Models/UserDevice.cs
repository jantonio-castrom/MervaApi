namespace MervaApi.UserTokens.Models;

public class UserDevice
{
    public int DeviceId { get; set; }
    public int TokenId { get; set; }
    public string? UserAgent { get; set; }
    public string? Browser { get; set; }
    public string? BrowserVersion { get; set; }
    public string? OperatingSystem { get; set; }
    public string? Language { get; set; }
    public string? Timezone { get; set; }
    public string? IpAddress { get; set; }
    public string? Country { get; set; }
    public string? Region { get; set; }
    public string? City { get; set; }
    public string? Isp { get; set; }
    public string? ConnectionType { get; set; }
    public DateTime RecordedAt { get; set; }

    public UserToken UserToken { get; set; } = null!;
}
