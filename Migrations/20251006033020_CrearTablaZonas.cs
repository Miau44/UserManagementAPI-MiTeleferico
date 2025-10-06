using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class CrearTablaZonas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Zonas",
                columns: table => new
                {
                    ZonaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreZona = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zonas", x => x.ZonaId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Zonas");
        }
    }
}
