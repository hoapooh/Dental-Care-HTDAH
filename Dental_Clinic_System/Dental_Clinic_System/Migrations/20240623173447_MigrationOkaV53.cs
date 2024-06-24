using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dental_Clinic_System.Migrations
{
    /// <inheritdoc />
    public partial class MigrationOkaV53 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    CompanyPhonenumber = table.Column<string>(type: "varchar(12)", nullable: false),
                    CompanyEmail = table.Column<string>(type: "varchar(50)", nullable: false),
                    RepresentativeName = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    ClinicName = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    ClinicAddress = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    DomainName = table.Column<string>(type: "varchar(200)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(2000)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.ID);
                    table.CheckConstraint("CK_CHECKVALID_STATUS", "Status = N'Chưa Duyệt' OR Status = N'Từ Chối' OR Status = N'Đồng Ý'");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
