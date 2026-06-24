using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _40kdb.Migrations
{
    /// <inheritdoc />
    public partial class AddDecalsApplied : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DecalsApplied",
                table: "Miniatures",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DecalsApplied",
                table: "Miniatures");
        }
    }
}
