using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profiles.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReceptionistStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "Receptionists",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Receptionists_status",
                table: "Receptionists",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Receptionists_status",
                table: "Receptionists");

            migrationBuilder.DropColumn(
                name: "status",
                table: "Receptionists");
        }
    }
}
