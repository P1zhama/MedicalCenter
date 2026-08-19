using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Appointments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNoOverlapConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            migrationBuilder.DropIndex(
                name: "IX_appointments_doctor_id_date_start_time",
                table: "appointments");

            migrationBuilder.Sql(
                """
                ALTER TABLE appointments
                ADD CONSTRAINT appointments_no_overlap
                EXCLUDE USING gist (
                    doctor_id WITH =,
                    tsrange(date + start_time, date + end_time) WITH &&
                ) WHERE (status <> 'Cancelled');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE appointments DROP CONSTRAINT appointments_no_overlap;");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_doctor_id_date_start_time",
                table: "appointments",
                columns: new[] { "doctor_id", "date", "start_time" },
                unique: true,
                filter: "status <> 'Cancelled'");
        }
    }
}
