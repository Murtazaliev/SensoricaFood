using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AVBDelivery.Migrations
{
    /// <inheritdoc />
    public partial class BackEnergy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "Energy",
                table: "Products",
                type: "real",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Carbs",
                table: "Products",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Fats",
                table: "Products",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Proteins",
                table: "Products",
                type: "real",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Carbs",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Fats",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Proteins",
                table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "Energy",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);
        }
    }
}
