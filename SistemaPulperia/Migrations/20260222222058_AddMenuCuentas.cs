using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SistemaPulperia.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuCuentas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Menus",
                columns: new[] { "Id", "Accion", "Activo", "Controlador", "Icono", "Nombre", "Orden", "PadreId" },
                values: new object[,]
                {
                    { 80, null, true, null, "bi bi-wallet2", "Gestión de Créditos", 5, null },
                    { 81, "Index", true, "Cuentas", "bi bi-person-badge", "Cuentas de Clientes", 1, 80 },
                    { 82, "Mora", true, "Cuentas", "bi bi-exclamation-octagon-fill", "Control de Mora", 2, 80 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 82);

            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 80);
        }
    }
}
