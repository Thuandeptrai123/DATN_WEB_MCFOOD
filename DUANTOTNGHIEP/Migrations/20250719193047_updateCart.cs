using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DUANTOTNGHIEP.Migrations
{
    /// <inheritdoc />
    public partial class updateCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Carts",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "FoodId",
                table: "CartItems",
                newName: "FoodID");

            migrationBuilder.RenameColumn(
                name: "ComboId",
                table: "CartItems",
                newName: "ComboID");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "CartItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "CartItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "CartItems");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Carts",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "FoodID",
                table: "CartItems",
                newName: "FoodId");

            migrationBuilder.RenameColumn(
                name: "ComboID",
                table: "CartItems",
                newName: "ComboId");
        }
    }
}
