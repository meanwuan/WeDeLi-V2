using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wedeli.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToTransportCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "user_id",
                table: "transport_companies",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_transport_companies_user_id",
                table: "transport_companies",
                column: "user_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "transport_companies_ibfk_user",
                table: "transport_companies",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "transport_companies_ibfk_user",
                table: "transport_companies");

            migrationBuilder.DropIndex(
                name: "IX_transport_companies_user_id",
                table: "transport_companies");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "transport_companies");
        }
    }
}
