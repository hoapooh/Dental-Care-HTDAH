using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dental_Clinic_System.Migrations
{
    /// <inheritdoc />
    public partial class MigrationOkaV50 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountID = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    Content = table.Column<string>(type: "ntext", nullable: true),
                    ThumbNail = table.Column<string>(type: "varchar(500)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    AccountID1 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_News", x => x.ID);
                    table.ForeignKey(
                        name: "FK__News__Account",
                        column: x => x.AccountID1,
                        principalTable: "Account",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_News_AccountID1",
                table: "News",
                column: "AccountID1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "News");
        }
    }
}
