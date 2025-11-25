namespace wedeli.Models.DTO
{
    public class ForgotPasswordRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}
