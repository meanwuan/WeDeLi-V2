using MySqlConnector;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var connectionString = "server=localhost;database=wedeli;user=root;password=Quangml@04";

        var sql = @"
CREATE TABLE IF NOT EXISTS `RefreshTokens` (
    `RefreshTokenId` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `Token` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
    `ExpiresAt` datetime NOT NULL,
    `CreatedAt` datetime NOT NULL,
    `IsRevoked` tinyint(1) NOT NULL DEFAULT 0,
    `RevokedAt` datetime NULL,
    CONSTRAINT `PK_RefreshTokens` PRIMARY KEY (`RefreshTokenId`),
    CONSTRAINT `FK_RefreshTokens_Users` FOREIGN KEY (`UserId`) REFERENCES `users` (`UserId`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

        var indexSql1 = @"CREATE INDEX IF NOT EXISTS `IX_RefreshTokens_UserId` ON `RefreshTokens` (`UserId`);";
        var indexSql2 = @"CREATE INDEX IF NOT EXISTS `IX_RefreshTokens_Token` ON `RefreshTokens` (`Token`);";

        try
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            Console.WriteLine("✓ Connected to database");

            using (var command = new MySqlCommand(sql, connection))
            {
                await command.ExecuteNonQueryAsync();
                Console.WriteLine("✓ RefreshTokens table created");
            }

            using (var command = new MySqlCommand(indexSql1, connection))
            {
                await command.ExecuteNonQueryAsync();
                Console.WriteLine("✓ Index on UserId created");
            }

            using (var command = new MySqlCommand(indexSql2, connection))
            {
                await command.ExecuteNonQueryAsync();
                Console.WriteLine("✓ Index on Token created");
            }

            Console.WriteLine("\n✓✓✓ All done! RefreshTokens table is ready.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return 1;
        }
    }
}
