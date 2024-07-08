using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dental_Clinic_System.Migrations
{
    /// <inheritdoc />
    public partial class MigrationV76Oka : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PatientRecord_MemberCard",
                table: "PatientRecord");

            migrationBuilder.DropColumn(
                name: "MemberCard",
                table: "PatientRecord");

            migrationBuilder.DropColumn(
                name: "Basis",
                table: "Clinic");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MemberCard",
                table: "PatientRecord",
                type: "nvarchar(15)",
                fixedLength: true,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Basis",
                table: "Clinic",
                type: "nvarchar(200)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientRecord_MemberCard",
                table: "PatientRecord",
                column: "MemberCard",
                unique: true,
                filter: "[MemberCard] IS NOT NULL");
        }
    }
}
