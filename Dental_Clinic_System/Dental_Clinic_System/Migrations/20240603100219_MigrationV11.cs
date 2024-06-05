using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dental_Clinic_System.Migrations
{
    /// <inheritdoc />
    public partial class MigrationV11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Account_Email",
                table: "Account");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Account_Email",
                table: "Account",
                column: "Email",
                unique: true);
        }
    }
}
