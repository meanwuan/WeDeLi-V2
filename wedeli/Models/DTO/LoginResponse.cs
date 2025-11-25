namespace wedeli.Models.DTO
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public UserResponse User { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}
