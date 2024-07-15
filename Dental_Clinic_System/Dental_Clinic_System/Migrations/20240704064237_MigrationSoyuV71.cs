using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dental_Clinic_System.Migrations
{
    /// <inheritdoc />
    public partial class MigrationSoyuV71 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    FutureAppointmentStatus = table.Column<string>(type: "nvarchar(30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FutureAppointment", x => x.ID);
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
                table: "PeriodicAppointments",
                column: "Dentist_ID");

            migrationBuilder.CreateIndex(
                name: "IX_FutureAppointments_PatientRecord_ID",
                table: "PeriodicAppointments",
                column: "PatientRecord_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeriodicAppointments");
        }
    }
}
