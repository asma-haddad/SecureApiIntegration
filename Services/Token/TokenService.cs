using ExpenseAuthApi.Data;
using ExpenseAuthApi.DTOs;
using ExpenseAuthApi.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAuthApi.Services.Token;

public class TokenService : ITokenService
{
    private readonly AppDbContext _context;

    private readonly AccessTokenService
        _accessTokenService;

    private readonly RefreshTokenService
        _refreshTokenService;

    public TokenService(
        AppDbContext context,
        AccessTokenService accessTokenService,
        RefreshTokenService refreshTokenService)
    {
        _context = context;

        _accessTokenService =
            accessTokenService;

        _refreshTokenService =
            refreshTokenService;
    }

    // ==========================
    // Get User + Create JWT
    // ==========================
    private async Task<(string token, DateTime expiresAtUtc)>
        CreateTokenAsync(int userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == userId);

        if (user == null)
        {
            throw new UserNotFoundException(
                "User not found");
        }

        return _accessTokenService
            .CreateToken(user);
    }

    // ==========================
    // Issue Access + Refresh
    // ==========================
    public async Task<TokenResponse>
        IssueTokensAsync(
            int userId,
            string? ip,
            string? userAgent)
    {
        var (accessToken, accessExpires) =
            await CreateTokenAsync(userId);

        var (refreshEntity, plainRefreshToken) =
            _refreshTokenService
                .CreateRefreshToken(
                    userId,
                    ip,
                    userAgent);

        _context.RefreshTokens
            .Add(refreshEntity);

        await _context.SaveChangesAsync();

        return new TokenResponse
        {
            AccessToken =
                accessToken,

            RefreshToken =
                plainRefreshToken,

            AccessTokenExpiresAtUtc =
                accessExpires,

            RefreshTokenExpiresAtUtc =
                refreshEntity.ExpiresAtUtc
        };
    }

    // ==========================
    // Refresh Token Rotation
    // ==========================
    public async Task<TokenResponse>
        RefreshAsync(
            string refreshToken,
            string? ip,
            string? userAgent)
    {
        var hash =
            _refreshTokenService
                .HashToken(refreshToken);

        var existing =
            await _context.RefreshTokens
                .FirstOrDefaultAsync(
                    x => x.TokenHash == hash);

        if (existing == null ||
            !existing.IsActive)
        {
            throw new UnAuthorizedException(
                "Invalid or expired refresh token");
        }

        // Create new Access Token
        var (accessToken, accessExpires) =
            await CreateTokenAsync(
                existing.UserId);

        // Create new Refresh Token
        var (
            newRefreshEntity,
            newPlainRefreshToken
        ) =
            _refreshTokenService
                .CreateRefreshToken(
                    existing.UserId,
                    ip,
                    userAgent);

        // Revoke old Refresh Token
        existing.RevokedAtUtc =
            DateTime.UtcNow;

        existing.ReplacedByTokenHash =
            newRefreshEntity.TokenHash;

        // Save new Refresh Token
        _context.RefreshTokens
            .Add(newRefreshEntity);

        await _context.SaveChangesAsync();

        return new TokenResponse
        {
            AccessToken =
                accessToken,

            RefreshToken =
                newPlainRefreshToken,

            AccessTokenExpiresAtUtc =
                accessExpires,

            RefreshTokenExpiresAtUtc =
                newRefreshEntity.ExpiresAtUtc
        };
    }
}