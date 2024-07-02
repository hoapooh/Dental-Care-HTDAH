using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dental_Clinic_System.Migrations
{
    /// <inheritdoc />
    public partial class MigrationLinaV67 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Valid_Schedule_Status",
                table: "Schedule");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Valid_Schedule_Status",
                table: "Schedule",
                sql: "ScheduleStatus = 'Booked' OR ScheduleStatus = 'Available' OR ScheduleStatus = N'Đã Đặt' OR ScheduleStatus = N'Còn Trống' OR ScheduleStatus = N'Lịch khám' OR ScheduleStatus = N'Lịch điều trị' OR ScheduleStatus = N'Lịch Sáng' OR ScheduleStatus = N'Lịch Chiều' OR ScheduleStatus = N'Nghỉ'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Valid_Schedule_Status",
                table: "Schedule");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Valid_Schedule_Status",
                table: "Schedule",
                sql: "ScheduleStatus = 'Booked' OR ScheduleStatus = 'Available' OR ScheduleStatus = N'Đã Đặt' OR ScheduleStatus = N'Còn Trống'");
        }
    }
}
