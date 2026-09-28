namespace PluriOne.Shared.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "plurione-auth";
    public string Audience { get; set; } = "plurione";
    public int AccessMinutes { get; set; } = 30;
    public int RefreshDays { get; set; } = 7;
}
