using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dental_Clinic_System.Migrations
{
    /// <inheritdoc />
    public partial class MigrationV16 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Appointment__PatientRecord",
                table: "Appointment");

            migrationBuilder.DropIndex(
                name: "IX_Appointment_PatientID",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "PatientID",
                table: "Appointment");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_PatientRecordID",
                table: "Appointment",
                column: "PatientRecordID");

            migrationBuilder.AddForeignKey(
                name: "FK__Appointment__PatientRecord",
                table: "Appointment",
                column: "PatientRecordID",
                principalTable: "PatientRecord",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Appointment__PatientRecord",
                table: "Appointment");

            migrationBuilder.DropIndex(
                name: "IX_Appointment_PatientRecordID",
                table: "Appointment");

            migrationBuilder.AddColumn<int>(
                name: "PatientID",
                table: "Appointment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_PatientID",
                table: "Appointment",
                column: "PatientID");

            migrationBuilder.AddForeignKey(
                name: "FK__Appointment__PatientRecord",
                table: "Appointment",
                column: "PatientID",
                principalTable: "PatientRecord",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
