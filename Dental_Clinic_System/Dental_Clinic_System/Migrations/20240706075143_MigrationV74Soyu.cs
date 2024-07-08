using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dental_Clinic_System.Migrations
{
    /// <inheritdoc />
    public partial class MigrationV74Soyu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPeriodic",
                table: "Appointment");

            migrationBuilder.AddColumn<int>(
                name: "Future_Appointment_ID",
                table: "Appointment",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Future_Appointment_ID",
                table: "Appointment");

            migrationBuilder.AddColumn<bool>(
                name: "IsPeriodic",
                table: "Appointment",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
