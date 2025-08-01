using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DUANTOTNGHIEP.Migrations
{
    /// <inheritdoc />
    public partial class cookQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CookedQuantity",
                table: "Foods",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CookedQuantity",
                table: "Foods");
        }
    }
}
