using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetSoLive.Data.Migrations
{
    /// <inheritdoc />
    public partial class locationupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSeenLocation",
                table: "LostPetAds");

            migrationBuilder.AddColumn<string>(
                name: "LastSeenCity",
                table: "LostPetAds",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSeenDistrict",
                table: "LostPetAds",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSeenCity",
                table: "LostPetAds");

            migrationBuilder.DropColumn(
                name: "LastSeenDistrict",
                table: "LostPetAds");

            migrationBuilder.AddColumn<string>(
                name: "LastSeenLocation",
                table: "LostPetAds",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
