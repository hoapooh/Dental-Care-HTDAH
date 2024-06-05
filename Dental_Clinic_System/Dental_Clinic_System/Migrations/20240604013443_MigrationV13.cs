using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dental_Clinic_System.Migrations
{
    /// <inheritdoc />
    public partial class MigrationV13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Valid_Status_Account",
                table: "Account");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Valid_Status_Account",
                table: "Account",
                sql: "[AccountStatus] = 'Active' OR [AccountStatus] = 'Banned' OR [AccountStatus] = N'Not Active' OR [AccountStatus] = N'Hoạt Động' OR [AccountStatus] = N'Bị Khóa' OR [AccountStatus] = N'Chưa Kích Hoạt'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Valid_Status_Account",
                table: "Account");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Valid_Status_Account",
                table: "Account",
                sql: "[AccountStatus] = 'Active' OR [AccountStatus] = 'Banned' OR [AccountStatus] = N'Hoạt Động' OR [AccountStatus] = N'Bị Khóa'");
        }
    }
}
