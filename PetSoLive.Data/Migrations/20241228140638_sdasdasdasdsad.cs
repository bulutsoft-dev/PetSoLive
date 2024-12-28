using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetSoLive.Data.Migrations
{
    /// <inheritdoc />
    public partial class sdasdasdasdsad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HelpRequests_Users_UserId1",
                table: "HelpRequests");

            migrationBuilder.DropIndex(
                name: "IX_HelpRequests_UserId1",
                table: "HelpRequests");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "HelpRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "HelpRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HelpRequests_UserId1",
                table: "HelpRequests",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_HelpRequests_Users_UserId1",
                table: "HelpRequests",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
