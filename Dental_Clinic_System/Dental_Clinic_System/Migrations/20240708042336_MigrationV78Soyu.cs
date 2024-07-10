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
                table: "FutureAppointments");

            migrationBuilder.AddCheckConstraint(
                name: "CK__Valid_FutureAppointmentStatus",
                table: "FutureAppointments",
                sql: "FutureAppointmentStatus = N'Chưa Khám' OR FutureAppointmentStatus = N'Đã Khám' OR FutureAppointmentStatus = N'Đã Hủy'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK__Valid_FutureAppointmentStatus",
                table: "FutureAppointments");

            migrationBuilder.AddCheckConstraint(
                name: "CK__Valid_FutureAppointmentStatus",
                table: "FutureAppointments",
                sql: "FutureAppointmentStatus = 'Chưa Khám' OR FutureAppointmentStatus = 'Đã Khám' OR FutureAppointmentStatus = 'Đã Hủy'");
        }
    }
}
