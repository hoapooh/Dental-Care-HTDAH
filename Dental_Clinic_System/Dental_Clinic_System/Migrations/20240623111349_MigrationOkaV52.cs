using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dental_Clinic_System.Migrations
{
    /// <inheritdoc />
    public partial class MigrationOkaV52 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__News__Account",
                table: "News");

            migrationBuilder.DropIndex(
                name: "IX_News_AccountID1",
                table: "News");

            migrationBuilder.DropColumn(
                name: "AccountID1",
                table: "News");

            migrationBuilder.CreateIndex(
                name: "IX_News_AccountID",
                table: "News",
                column: "AccountID");

            migrationBuilder.AddForeignKey(
                name: "FK__News__Account",
                table: "News",
                column: "AccountID",
                principalTable: "Account",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__News__Account",
                table: "News");

            migrationBuilder.DropIndex(
                name: "IX_News_AccountID",
                table: "News");

            migrationBuilder.AddColumn<int>(
                name: "AccountID1",
                table: "News",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_News_AccountID1",
                table: "News",
                column: "AccountID1");

            migrationBuilder.AddForeignKey(
                name: "FK__News__Account",
                table: "News",
                column: "AccountID1",
                principalTable: "Account",
                principalColumn: "ID");
        }
    }
}
