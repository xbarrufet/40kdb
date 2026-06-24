using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _40kdb.Migrations
{
    /// <inheritdoc />
    public partial class AddPrimedState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Shift existing Painted (2) to 3, since Primed is now 2
            migrationBuilder.Sql("UPDATE Miniatures SET State = 3 WHERE State = 2");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Shift Painted back from 3 to 2
            migrationBuilder.Sql("UPDATE Miniatures SET State = 2 WHERE State = 3");
        }
    }
}
