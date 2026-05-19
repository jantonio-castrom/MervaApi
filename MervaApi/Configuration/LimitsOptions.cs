namespace MervaApi.Configuration;

public class LimitsOptions
{
    public const string Section = "Limits";
    public int FreeTransactionLimit { get; set; } = 50;
}
