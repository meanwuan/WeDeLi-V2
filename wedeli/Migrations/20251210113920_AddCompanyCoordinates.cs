using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wedeli.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "latitude",
                table: "transport_companies",
                type: "decimal(10,8)",
                precision: 10,
                scale: 8,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "longitude",
                table: "transport_companies",
                type: "decimal(11,8)",
                precision: 11,
                scale: 8,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "latitude",
                table: "transport_companies");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "transport_companies");
        }
    }
}
