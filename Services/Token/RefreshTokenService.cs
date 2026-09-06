using ExpenseAuthApi.Model;
using System.Security.Cryptography;
using System.Text;

namespace ExpenseAuthApi.Services.Token;

public class RefreshTokenService
{
    private readonly IConfiguration _configuration;

    public RefreshTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (RefreshToken entity, string plainToken) CreateRefreshToken(
        int userId,
        string? ip,
        string? userAgent)
    {
        var bytes = RandomNumberGenerator.GetBytes(64);

        var plainToken = Convert.ToBase64String(bytes);

        var tokenHash = HashToken(plainToken);

        var refreshTokenDays = int.TryParse(
            _configuration["Jwt:RefreshTokenDays"],
            out var days)
                ? days
                : 7;

        var entity = new RefreshToken
        {
            UserId = userId,

            TokenHash = tokenHash,

            CreatedAtUtc = DateTime.UtcNow,

            ExpiresAtUtc =
                DateTime.UtcNow.AddDays(refreshTokenDays),

            CreatedByIp = ip,

            UserAgent = userAgent
        };

        return (entity, plainToken);
    }

    public string HashToken(string token)
    {
        using var sha = SHA256.Create();

        var bytes = sha.ComputeHash(
            Encoding.UTF8.GetBytes(token));

        return Convert.ToHexString(bytes);
    }
}