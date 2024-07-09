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
                table: "FutureAppointments",
                type: "nvarchar(1000)",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK__Valid_FutureAppointmentStatus",
                table: "FutureAppointments",
                sql: "FutureAppointmentStatus = 'Chưa Khám' OR FutureAppointmentStatus = 'Đã Khám' OR FutureAppointmentStatus = 'Đã Hủy'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK__Valid_FutureAppointmentStatus",
                table: "FutureAppointments");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "FutureAppointments");
        }
    }
}
