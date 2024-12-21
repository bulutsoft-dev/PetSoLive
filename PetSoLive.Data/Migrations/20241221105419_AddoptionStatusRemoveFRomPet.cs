using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetSoLive.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddoptionStatusRemoveFRomPet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdoptionStatus",
                table: "Pets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdoptionStatus",
                table: "Pets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
