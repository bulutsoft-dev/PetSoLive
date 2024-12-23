using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetSoLive.Data.Migrations
{
    /// <inheritdoc />
    public partial class User : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PetId1",
                table: "AdoptionRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionRequests_PetId1",
                table: "AdoptionRequests",
                column: "PetId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AdoptionRequests_Pets_PetId1",
                table: "AdoptionRequests",
                column: "PetId1",
                principalTable: "Pets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdoptionRequests_Pets_PetId1",
                table: "AdoptionRequests");

            migrationBuilder.DropIndex(
                name: "IX_AdoptionRequests_PetId1",
                table: "AdoptionRequests");

            migrationBuilder.DropColumn(
                name: "PetId1",
                table: "AdoptionRequests");
        }
    }
}
