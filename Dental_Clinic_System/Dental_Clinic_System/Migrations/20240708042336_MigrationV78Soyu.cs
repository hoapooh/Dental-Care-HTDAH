using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dental_Clinic_System.Migrations
{
    /// <inheritdoc />
    public partial class MigrationV78Soyu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK__Valid_FutureAppointmentStatus",
                table: "PeriodicAppointments");

            migrationBuilder.AddCheckConstraint(
                name: "CK__Valid_FutureAppointmentStatus",
                table: "PeriodicAppointments",
                sql: "PeriodicAppointmentStatus = N'Chưa Khám' OR PeriodicAppointmentStatus = N'Đã Khám' OR PeriodicAppointmentStatus = N'Đã Hủy'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK__Valid_FutureAppointmentStatus",
                table: "PeriodicAppointments");

            migrationBuilder.AddCheckConstraint(
                name: "CK__Valid_FutureAppointmentStatus",
                table: "PeriodicAppointments",
                sql: "PeriodicAppointmentStatus = 'Chưa Khám' OR PeriodicAppointmentStatus = 'Đã Khám' OR PeriodicAppointmentStatus = 'Đã Hủy'");
        }
    }
}
