namespace ExpenseAuthApi.Model
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string TokenHash { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAtUtc { get; set; }

        public DateTime? RevokedAtUtc { get; set; }

        public string? ReplacedByTokenHash { get; set; }

        public string? CreatedByIp { get; set; }

        public string? UserAgent { get; set; }

        public User User { get; set; } = null!;

        public bool IsActive =>
            RevokedAtUtc == null &&
            ExpiresAtUtc > DateTime.UtcNow;
    }
}
