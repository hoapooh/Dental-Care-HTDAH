using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dental_Clinic_System.Migrations
{
    /// <inheritdoc />
    public partial class MigrationV14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLinked",
                table: "Account",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLinked",
                table: "Account");
        }
    }
}
