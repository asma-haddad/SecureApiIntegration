using ExpenseAuthApi.DTOs;

namespace ExpenseAuthApi.Services
{
    public interface ITokenService
    {
        Task<TokenResponse> IssueTokensAsync(
        int userId,
        string? ip,
        string? userAgent);

        Task<TokenResponse> RefreshAsync(
        string refreshToken,
        string? ip,
        string? userAgent);
    }

}