using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dental_Clinic_System.Migrations
{
    /// <inheritdoc />
    public partial class MigrationV82Soyu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FutureAppointments");

            migrationBuilder.CreateTable(
                name: "PeriodicAppointments",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientRecord_ID = table.Column<int>(type: "int", nullable: false),
                    Dentist_ID = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time(7)", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time(7)", nullable: false),
                    DesiredDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", nullable: true),
                    PeriodicAppointmentStatus = table.Column<string>(type: "nvarchar(30)", nullable: true),
                    AppointmentID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicAppointment", x => x.ID);
                    table.CheckConstraint("CK__Valid_PeriodicAppointmentStatus", "PeriodicAppointmentStatus = N'Đã Chấp Nhận' OR PeriodicAppointmentStatus = N'Đã Khám' OR PeriodicAppointmentStatus = N'Đã Hủy'");
                    table.ForeignKey(
                        name: "FK__Dentist__PeriodicAppointments",
                        column: x => x.Dentist_ID,
                        principalTable: "Dentist",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK__PatientRecord__PeriodicAppointments",
                        column: x => x.PatientRecord_ID,
                        principalTable: "PatientRecord",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicAppointments_Dentist_ID",
                table: "PeriodicAppointments",
                column: "Dentist_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicAppointments_PatientRecord_ID",
                table: "PeriodicAppointments",
                column: "PatientRecord_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropTable(
                name: "PeriodicAppointments");

            migrationBuilder.CreateTable(
                name: "FutureAppointments",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Dentist_ID = table.Column<int>(type: "int", nullable: false),
                    PatientRecord_ID = table.Column<int>(type: "int", nullable: false),
                    AppointmentID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", nullable: true),
                    DesiredDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time(7)", nullable: false),
                    FutureAppointmentStatus = table.Column<string>(type: "nvarchar(30)", nullable: true),
                    StartTime = table.Column<TimeOnly>(type: "time(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FutureAppointment", x => x.ID);
                    table.CheckConstraint("CK__Valid_FutureAppointmentStatus", "FutureAppointmentStatus = N'Chưa Khám' OR FutureAppointmentStatus = N'Đã Khám' OR FutureAppointmentStatus = N'Đã Hủy'");
                    table.ForeignKey(
                        name: "FK__Dentist__FutureAppointments",
                        column: x => x.Dentist_ID,
                        principalTable: "Dentist",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK__PatientRecord__FutureAppointments",
                        column: x => x.PatientRecord_ID,
                        principalTable: "PatientRecord",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FutureAppointments_Dentist_ID",
                table: "FutureAppointments",
                column: "Dentist_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FutureAppointments_PatientRecord_ID",
                table: "FutureAppointments",
                column: "PatientRecord_ID");
        }
    }
}
