using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DUANTOTNGHIEP.Migrations
{
    /// <inheritdoc />
    public partial class DATNFIX : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Foods",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Foods");
        }
    }
}
