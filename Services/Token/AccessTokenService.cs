using ExpenseAuthApi.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExpenseAuthApi.Services.Token
{
    public class AccessTokenService
    {
        private readonly IConfiguration _configuration;

        public AccessTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (string token, DateTime expiresAtUtc)
            CreateToken(User user)
        {
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
    }
}
