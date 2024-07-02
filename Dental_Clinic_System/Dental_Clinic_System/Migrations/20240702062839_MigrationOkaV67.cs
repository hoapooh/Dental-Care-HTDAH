using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dental_Clinic_System.Migrations
{
    /// <inheritdoc />
    public partial class MigrationOkaV67 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Valid_Schedule_Status",
                table: "Schedule");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Orders",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Valid_Schedule_Status",
                table: "Schedule",
                sql: "ScheduleStatus = 'Booked' OR ScheduleStatus = 'Available' OR ScheduleStatus = N'Đã Đặt' OR ScheduleStatus = N'Còn Trống' OR ScheduleStatus = N'Lịch khám' OR ScheduleStatus = N'Lịch điều trị' OR ScheduleStatus = N'Lịch Đã Tạo' ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Valid_Schedule_Status",
                table: "Schedule");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Orders");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Valid_Schedule_Status",
                table: "Schedule",
                sql: "ScheduleStatus = 'Booked' OR ScheduleStatus = 'Available' OR ScheduleStatus = N'Đã Đặt' OR ScheduleStatus = N'Còn Trống'");
        }
    }
}
