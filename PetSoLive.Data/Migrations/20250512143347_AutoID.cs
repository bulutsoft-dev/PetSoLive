using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetSoLive.Data.Migrations
{
    /// <inheritdoc />
    public partial class AutoID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adoptions tablosunun Id sequence'ını düzelt
            migrationBuilder.Sql("SELECT setval('\"Adoptions_Id_seq\"', (SELECT MAX(\"Id\") FROM \"Adoptions\"));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
