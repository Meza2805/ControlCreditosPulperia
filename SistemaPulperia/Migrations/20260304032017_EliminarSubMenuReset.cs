using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaPulperia.Migrations
{
    /// <inheritdoc />
    public partial class EliminarSubMenuReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.AlterColumn<string>(
                name: "UsuarioId",
                table: "Transacciones",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Transacciones_UsuarioId",
                table: "Transacciones",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transacciones_AspNetUsers_UsuarioId",
                table: "Transacciones",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transacciones_AspNetUsers_UsuarioId",
                table: "Transacciones");

            migrationBuilder.DropIndex(
                name: "IX_Transacciones_UsuarioId",
                table: "Transacciones");

            migrationBuilder.AlterColumn<string>(
                name: "UsuarioId",
                table: "Transacciones",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.InsertData(
                table: "Menus",
                columns: new[] { "Id", "Accion", "Activo", "Controlador", "Icono", "Nombre", "Orden", "PadreId" },
                values: new object[] { 62, "ResetPassword", true, "Account", "bi bi-key-fill", "Restablecer Contraseña", 2, 60 });
        }
    }
}
