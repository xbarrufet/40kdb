using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _40kdb.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginalAndProxy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Original",
                table: "Miniatures",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Proxy",
                table: "Miniatures",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Original",
                table: "Miniatures");

            migrationBuilder.DropColumn(
                name: "Proxy",
                table: "Miniatures");
        }
    }
}
