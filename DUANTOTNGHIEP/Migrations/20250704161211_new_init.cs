using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DUANTOTNGHIEP.Migrations
{
    /// <inheritdoc />
    public partial class new_init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PhoneNumbers",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumbers",
                table: "AspNetUsers");
        }
    }
}
