using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dental_Clinic_System.Migrations
{
    /// <inheritdoc />
    public partial class MigrationV77Soyu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PeriodicAppointments",
                type: "nvarchar(1000)",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK__Valid_FutureAppointmentStatus",
                table: "PeriodicAppointments",
                sql: "PeriodicAppointmentStatus = 'Chưa Khám' OR PeriodicAppointmentStatus = 'Đã Khám' OR PeriodicAppointmentStatus = 'Đã Hủy'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK__Valid_FutureAppointmentStatus",
                table: "PeriodicAppointments");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PeriodicAppointments");
        }
    }
}
