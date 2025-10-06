using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogsV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileImage",
                table: "Users");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "ProfileImage",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
