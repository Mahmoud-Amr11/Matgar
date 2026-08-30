namespace Matgar.Infrastructure.Options
{
    internal sealed class JwtOptions
    {
        public const string SectionName = "JwtOptions";
        public string Key { get; init; } = string.Empty;
        public string Issuer { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
        public int AccessTokenDurationMinutes { get; init; }
        public int RefreshTokenDurationDays { get; init; }
    }
}
