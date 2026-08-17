using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AVBDelivery.Migrations
{
    /// <inheritdoc />
    public partial class _202503263 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderComment",
                table: "Orders");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinimalSum",
                table: "Organizations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "MinimalSum",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Organizations");

            migrationBuilder.AddColumn<string>(
                name: "OrderComment",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
