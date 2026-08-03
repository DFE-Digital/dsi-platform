using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace migrations.Directories
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToUserEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IDX__user__email__unique",
                table: "user",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IDX__user__email__unique",
                table: "user");
        }
    }
}
