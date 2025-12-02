using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wedeli.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokensTableOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
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
                ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS `IX_RefreshTokens_UserId` ON `RefreshTokens` (`UserId`);
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS `IX_RefreshTokens_Token` ON `RefreshTokens` (`Token`);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS `RefreshTokens`;");
        }
    }
}
