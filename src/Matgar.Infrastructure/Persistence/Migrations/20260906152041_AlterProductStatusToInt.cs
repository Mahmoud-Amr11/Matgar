using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Matgar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlterProductStatusToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // تحويل العمود إلى int
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Products",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)", // أو nvarchar(50) حسب ما كان موجوداً في الداتابيز
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // إرجاع العمود إلى نص في حالة التراجع عن التحديث
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
