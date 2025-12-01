namespace wedeli.Infrastructure
{
    public class PasswordService : IPasswordService
    {/// <summary>
     /// Hash password using BCrypt
     /// </summary>
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        /// <summary>
        /// Verify password against hash
        /// </summary>
        public bool VerifyPassword(string password, string passwordHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generate random password
        /// </summary>
        public string GenerateRandomPassword(int length = 12)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
            var random = new Random();
            var chars = new char[length];

            // Đảm bảo có ít nhất 1 uppercase, 1 lowercase, 1 digit, 1 special char
            chars[0] = validChars[random.Next(26, 52)]; // Uppercase
            chars[1] = validChars[random.Next(0, 26)];   // Lowercase
            chars[2] = validChars[random.Next(52, 62)];  // Digit
            chars[3] = validChars[random.Next(62, validChars.Length)]; // Special

            // Fill remaining characters
            for (int i = 4; i < length; i++)
            {
                chars[i] = validChars[random.Next(validChars.Length)];
            }

            // Shuffle
            return new string(chars.OrderBy(x => random.Next()).ToArray());
        }

        /// <summary>
        /// Check if password meets strength requirements
        /// </summary>
        public bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                return false;

            bool hasUpperCase = password.Any(char.IsUpper);
            bool hasLowerCase = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);

            return hasUpperCase && hasLowerCase && hasDigit;
        }
    }
}
