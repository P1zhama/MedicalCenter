using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Documents.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameConfirmedAtToLastVerifiedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_documents_confirmed_at",
                table: "documents");

            migrationBuilder.RenameColumn(
                name: "confirmed_at",
                table: "documents",
                newName: "last_verified_at");

            migrationBuilder.CreateIndex(
                name: "IX_documents_last_verified_at_created_at",
                table: "documents",
                columns: new[] { "last_verified_at", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_documents_last_verified_at_created_at",
                table: "documents");

            migrationBuilder.RenameColumn(
                name: "last_verified_at",
                table: "documents",
                newName: "confirmed_at");

            migrationBuilder.CreateIndex(
                name: "IX_documents_confirmed_at",
                table: "documents",
                column: "confirmed_at");
        }
    }
}
