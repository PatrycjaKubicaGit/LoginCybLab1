using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoginCybLab1.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PasswordHistories_AspNetUsers_UserId",
                table: "PasswordHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PasswordHistories",
                table: "PasswordHistories");

            migrationBuilder.RenameTable(
                name: "PasswordHistories",
                newName: "PasswordHistory");

            migrationBuilder.RenameIndex(
                name: "IX_PasswordHistories_UserId",
                table: "PasswordHistory",
                newName: "IX_PasswordHistory_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PasswordHistory",
                table: "PasswordHistory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordHistory_AspNetUsers_UserId",
                table: "PasswordHistory",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PasswordHistory_AspNetUsers_UserId",
                table: "PasswordHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PasswordHistory",
                table: "PasswordHistory");

            migrationBuilder.RenameTable(
                name: "PasswordHistory",
                newName: "PasswordHistories");

            migrationBuilder.RenameIndex(
                name: "IX_PasswordHistory_UserId",
                table: "PasswordHistories",
                newName: "IX_PasswordHistories_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PasswordHistories",
                table: "PasswordHistories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordHistories_AspNetUsers_UserId",
                table: "PasswordHistories",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
