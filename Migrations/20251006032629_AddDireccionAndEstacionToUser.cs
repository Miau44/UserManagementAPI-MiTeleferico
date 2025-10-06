using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDireccionAndEstacionToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Calle",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstacionAsignada",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitud",
                table: "Users",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LineaAsignada",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitud",
                table: "Users",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZonaSeleccionada",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Estaciones",
                columns: table => new
                {
                    EstacionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Linea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitud = table.Column<double>(type: "float", nullable: false),
                    Longitud = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estaciones", x => x.EstacionId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Estaciones");

            migrationBuilder.DropColumn(
                name: "Calle",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EstacionAsignada",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Latitud",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LineaAsignada",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Longitud",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ZonaSeleccionada",
                table: "Users");
        }
    }
}
