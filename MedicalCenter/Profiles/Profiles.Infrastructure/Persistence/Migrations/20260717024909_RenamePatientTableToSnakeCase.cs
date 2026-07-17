using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profiles.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenamePatientTableToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Patient",
                table: "Patient");

            migrationBuilder.DropIndex(
                name: "IX_Patient_AccountId",
                table: "Patient");

            migrationBuilder.DropIndex(
                name: "IX_Patient_PhoneNumber",
                table: "Patient");

            migrationBuilder.RenameTable(
                name: "Patient",
                newName: "Patients");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Patients",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Patients",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "PhotoUrl",
                table: "Patients",
                newName: "photo_url");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Patients",
                newName: "phone_number");

            migrationBuilder.RenameColumn(
                name: "MiddleName",
                table: "Patients",
                newName: "middle_name");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Patients",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Patients",
                newName: "first_name");

            migrationBuilder.RenameColumn(
                name: "DateOfBirth",
                table: "Patients",
                newName: "date_of_birth");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Patients",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "Patients",
                newName: "account_id");

            migrationBuilder.RenameIndex(
                name: "IX_Patient_LastName_FirstName_DateOfBirth",
                table: "Patients",
                newName: "IX_Patients_last_name_first_name_date_of_birth");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Patients",
                table: "Patients",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_account_id",
                table: "Patients",
                column: "account_id",
                unique: true,
                filter: "[account_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_phone_number",
                table: "Patients",
                column: "phone_number",
                unique: true,
                filter: "[phone_number] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Patients",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_account_id",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_phone_number",
                table: "Patients");

            migrationBuilder.RenameTable(
                name: "Patients",
                newName: "Patient");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Patient",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Patient",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "photo_url",
                table: "Patient",
                newName: "PhotoUrl");

            migrationBuilder.RenameColumn(
                name: "phone_number",
                table: "Patient",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "middle_name",
                table: "Patient",
                newName: "MiddleName");

            migrationBuilder.RenameColumn(
                name: "last_name",
                table: "Patient",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "first_name",
                table: "Patient",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "date_of_birth",
                table: "Patient",
                newName: "DateOfBirth");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Patient",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "account_id",
                table: "Patient",
                newName: "AccountId");

            migrationBuilder.RenameIndex(
                name: "IX_Patients_last_name_first_name_date_of_birth",
                table: "Patient",
                newName: "IX_Patient_LastName_FirstName_DateOfBirth");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Patient",
                table: "Patient",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Patient_AccountId",
                table: "Patient",
                column: "AccountId",
                unique: true,
                filter: "[AccountId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Patient_PhoneNumber",
                table: "Patient",
                column: "PhoneNumber",
                unique: true,
                filter: "[PhoneNumber] IS NOT NULL");
        }
    }
}
