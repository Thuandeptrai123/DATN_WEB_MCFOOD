using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DUANTOTNGHIEP.Migrations
{
    /// <inheritdoc />
    public partial class cookable_quantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CookableQuantity",
                table: "Foods",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CookableQuantity",
                table: "Foods");
        }
    }
}
