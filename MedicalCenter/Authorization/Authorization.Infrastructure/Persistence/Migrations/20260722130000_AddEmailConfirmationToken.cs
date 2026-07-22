using System;
using Authorization.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Authorization.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AuthDbContext))]
    [Migration("20260722130000_AddEmailConfirmationToken")]
    public partial class AddEmailConfirmationToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "email_confirmation_token_hash",
                table: "Accounts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "email_confirmation_token_expires_at",
                table: "Accounts",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_email_confirmation_token_hash",
                table: "Accounts",
                column: "email_confirmation_token_hash");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_email_confirmation_token_hash",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "email_confirmation_token_hash",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "email_confirmation_token_expires_at",
                table: "Accounts");
        }
    }
}
