namespace wedeli.Models.Domain
{
    public class RefreshToken
    {
        public int RefreshTokenId { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRevoked { get; set; }
        public string? RevokedReason { get; set; }
        public DateTime? RevokedAt { get; set; }

        // Navigation property
        public User User { get; set; } = null!;
    }
}
