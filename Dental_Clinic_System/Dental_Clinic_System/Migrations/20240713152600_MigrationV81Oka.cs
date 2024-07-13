using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dental_Clinic_System.Migrations
{
    /// <inheritdoc />
    public partial class MigrationV81Oka : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Valid_Role",
                table: "Account");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Valid_Role",
                table: "Account",
                sql: "[Role] = 'Admin' OR [Role] = 'PatientRecord' OR [Role] = 'Dentist' OR [Role] = 'Manager' OR [Role] = N'Bệnh Nhân' OR [Role] = N'Nha Sĩ' OR [Role] = N'Quản Lý' OR [Role] = N'Mini Admin'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Valid_Role",
                table: "Account");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Valid_Role",
                table: "Account",
                sql: "[Role] = 'Admin' OR [Role] = 'PatientRecord' OR [Role] = 'Dentist' OR [Role] = 'Manager' OR [Role] = N'Bệnh Nhân' OR [Role] = N'Nha Sĩ' OR [Role] = N'Quản Lý'");
        }
    }
}
