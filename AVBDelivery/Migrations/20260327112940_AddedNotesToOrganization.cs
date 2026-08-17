using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AVBDelivery.Migrations
{
    /// <inheritdoc />
    public partial class AddedNotesToOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NoteAmoCrmId",
                table: "Organizations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoteAmoCrmId",
                table: "Organizations");
        }
    }
}
