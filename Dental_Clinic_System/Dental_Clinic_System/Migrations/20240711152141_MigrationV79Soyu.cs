using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dental_Clinic_System.Migrations
{
    /// <inheritdoc />
    public partial class MigrationV79Soyu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Valid_Schedule_Status",
                table: "Schedule");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Valid_Status_Appointment",
                table: "Appointment");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Valid_Gender",
                table: "Account");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Valid_Status_Account",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "Future_Appointment_ID",
                table: "Appointment");

            migrationBuilder.AddColumn<int>(
                name: "AppointmentID",
                table: "FutureAppointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsExport",
                table: "Appointment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Valid_Schedule_Status",
                table: "Schedule",
                sql: "ScheduleStatus = N'Đã Đặt' OR ScheduleStatus = N'Còn Trống' OR ScheduleStatus = N'Lịch khám' OR ScheduleStatus = N'Lịch điều trị' OR ScheduleStatus = N'Lịch Sáng' OR ScheduleStatus = N'Lịch Chiều' OR ScheduleStatus = N'Nghỉ' OR ScheduleStatus = N'Đã Hủy'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Valid_Status_Appointment",
                table: "Appointment",
                sql: "[AppointmentStatus] = N'Chờ Xác Nhận' OR [AppointmentStatus] = N'Đã Hủy' OR [AppointmentStatus] = N'Đã Chấp Nhận' OR [AppointmentStatus] = N'Đã Khám'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Valid_Gender",
                table: "Account",
                sql: "[Gender] = N'Nam' OR [Gender] = N'Nữ'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Valid_Status_Account",
                table: "Account",
                sql: "[AccountStatus] = N'Hoạt Động' OR [AccountStatus] = N'Bị Khóa' OR [AccountStatus] = N'Chưa Kích Hoạt'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Valid_Schedule_Status",
                table: "Schedule");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Valid_Status_Appointment",
                table: "Appointment");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Valid_Gender",
                table: "Account");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Valid_Status_Account",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "AppointmentID",
                table: "FutureAppointments");

            migrationBuilder.DropColumn(
                name: "IsExport",
                table: "Appointment");

            migrationBuilder.AddColumn<int>(
                name: "Future_Appointment_ID",
                table: "Appointment",
                type: "int",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Valid_Schedule_Status",
                table: "Schedule",
                sql: "ScheduleStatus = 'Booked' OR ScheduleStatus = 'Available' OR ScheduleStatus = N'Đã Đặt' OR ScheduleStatus = N'Còn Trống' OR ScheduleStatus = N'Lịch khám' OR ScheduleStatus = N'Lịch điều trị' OR ScheduleStatus = N'Lịch Sáng' OR ScheduleStatus = N'Lịch Chiều' OR ScheduleStatus = N'Nghỉ' OR ScheduleStatus = N'Đã Hủy'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Valid_Status_Appointment",
                table: "Appointment",
                sql: "[AppointmentStatus] = 'Pending' OR [AppointmentStatus] = 'Canceled' OR [AppointmentStatus] = 'Approved' OR [AppointmentStatus] = 'Completed' OR [AppointmentStatus] = N'Chờ Xác Nhận' OR [AppointmentStatus] = N'Đã Hủy' OR [AppointmentStatus] = N'Đã Chấp Nhận' OR [AppointmentStatus] = N'Đã Khám'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Valid_Gender",
                table: "Account",
                sql: "[Gender] = 'Male' OR [Gender] = 'Female' OR [Gender] = N'Nam' OR [Gender] = N'Nữ'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Valid_Status_Account",
                table: "Account",
                sql: "[AccountStatus] = 'Active' OR [AccountStatus] = 'Banned' OR [AccountStatus] = N'Not Active' OR [AccountStatus] = N'Hoạt Động' OR [AccountStatus] = N'Bị Khóa' OR [AccountStatus] = N'Chưa Kích Hoạt'");
        }
    }
}
