using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AVBDelivery.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoteAmoCrmId",
                table: "Organizations");

            migrationBuilder.CreateTable(
                name: "NoteOrganization",
                columns: table => new
                {
                    NotesId = table.Column<int>(type: "int", nullable: false),
                    OrganizationsOrganizationId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteOrganization", x => new { x.NotesId, x.OrganizationsOrganizationId });
                    table.ForeignKey(
                        name: "FK_NoteOrganization_Notes_NotesId",
                        column: x => x.NotesId,
                        principalTable: "Notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NoteOrganization_Organizations_OrganizationsOrganizationId",
                        column: x => x.OrganizationsOrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "OrganizationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NoteOrganization_OrganizationsOrganizationId",
                table: "NoteOrganization",
                column: "OrganizationsOrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NoteOrganization");

            migrationBuilder.AddColumn<int>(
                name: "NoteAmoCrmId",
                table: "Organizations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
