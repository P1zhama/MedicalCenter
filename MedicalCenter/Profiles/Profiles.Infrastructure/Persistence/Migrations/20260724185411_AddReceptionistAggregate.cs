using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profiles.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReceptionistAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "Receptionists",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "created_by",
                table: "Receptionists",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "Receptionists",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by",
                table: "Receptionists",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "version",
                table: "Receptionists",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "Receptionists");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "Receptionists");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "Receptionists");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "Receptionists");

            migrationBuilder.DropColumn(
                name: "version",
                table: "Receptionists");
        }
    }
}
