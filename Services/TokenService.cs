using ExpenseAuthApi.Data;
using ExpenseAuthApi.DTOs;
using ExpenseAuthApi.Exceptions;
using ExpenseAuthApi.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ExpenseAuthApi.Services.Token;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;

    public TokenService(
        IConfiguration configuration,
        AppDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    // ==========================
    // Create Access Token (JWT)
    // ==========================
    private async Task<(string token, DateTime expiresAtUtc)>
        CreateTokenAsync(int userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
            throw new UserNotFoundException("User not found");

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Email,
                user.Email),

            new Claim(
                ClaimTypes.Role,
                user.Role)
        };

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT Issuer is missing");

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT Audience is missing");

        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT Key is missing");

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));

        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var durationMinutes = int.TryParse(
            _configuration["Jwt:AccessTokenMinutes"],
            out var minutes)
                ? minutes
                : 30;

        var expires =
            DateTime.UtcNow.AddMinutes(durationMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var jwt = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return (jwt, expires);
    }

    // ==========================
    // Create Refresh Token
    // ==========================
    private (RefreshToken entity, string plainToken)
        CreateRefreshToken(
            int userId,
            string? ip,
            string? userAgent)
    {
        // Generate secure random refresh token
        var bytes =
            RandomNumberGenerator.GetBytes(64);

        var plainToken =
            Convert.ToBase64String(bytes);

        // We store only the hash in the database
        var tokenHash =
            Sha256(plainToken);

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

    // ==========================
    // Issue Access + Refresh
    // Called after successful Login
    // ==========================
    public async Task<TokenResponse> IssueTokensAsync(
        int userId,
        string? ip,
        string? userAgent)
    {
        // Create JWT
        var (accessToken, accessExpires) =
            await CreateTokenAsync(userId);

        // Create Refresh Token
        var (refreshEntity, plainRefreshToken) =
            CreateRefreshToken(
                userId,
                ip,
                userAgent);

        // Save only Refresh Token Hash
        _context.RefreshTokens.Add(refreshEntity);

        await _context.SaveChangesAsync();

        // Return plain tokens to client
        return new TokenResponse
        {
            AccessToken = accessToken,

            RefreshToken = plainRefreshToken,

            AccessTokenExpiresAtUtc =
                accessExpires,

            RefreshTokenExpiresAtUtc =
                refreshEntity.ExpiresAtUtc
        };
    }

    // ==========================
    // Refresh Token Rotation
    // ==========================
    public async Task<TokenResponse> RefreshAsync(
        string refreshToken,
        string? ip,
        string? userAgent)
    {
        // Client sends the plain refresh token.
        // We hash it because DB contains only hashes.
        var hash = Sha256(refreshToken);

        var existing = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == hash);

        // Token doesn't exist, expired or revoked
        if (existing == null || !existing.IsActive)
        {
            throw new UnAuthorizedException(
                "Invalid or expired refresh token");
        }

        // Create a new Access Token
        var (accessToken, accessExpires) =
            await CreateTokenAsync(existing.UserId);

        // Create a NEW Refresh Token
        var (newRefreshEntity, newPlainRefreshToken) =
            CreateRefreshToken(
                existing.UserId,
                ip,
                userAgent);

        // Revoke the old Refresh Token
        existing.RevokedAtUtc = DateTime.UtcNow;

        // Keep track of the token that replaced it
        existing.ReplacedByTokenHash =
            newRefreshEntity.TokenHash;

        // Save new Refresh Token hash
        _context.RefreshTokens.Add(newRefreshEntity);

        await _context.SaveChangesAsync();

        return new TokenResponse
        {
            AccessToken = accessToken,

            RefreshToken = newPlainRefreshToken,

            AccessTokenExpiresAtUtc =
                accessExpires,

            RefreshTokenExpiresAtUtc =
                newRefreshEntity.ExpiresAtUtc
        };
    }

    // ==========================
    // Hash Refresh Token
    // ==========================
    private static string Sha256(string input)
    {
        using var sha = SHA256.Create();

        var bytes = sha.ComputeHash(
            Encoding.UTF8.GetBytes(input));

        return Convert.ToHexString(bytes);
    }
}