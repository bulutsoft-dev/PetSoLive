using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetSoLive.Data.Migrations
{
    /// <inheritdoc />
    public partial class ee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "PetOwners");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "PetOwners",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
