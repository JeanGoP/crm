using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductoFotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(220)", maxLength: 220, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    Datos = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    EsPrincipalCotizacion = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UsuarioActualizacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductoFotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductoFotos_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductoFotos_EmpresaId",
                table: "ProductoFotos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoFotos_EmpresaId_ProductoId",
                table: "ProductoFotos",
                columns: new[] { "EmpresaId", "ProductoId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductoFotos_ProductoId",
                table: "ProductoFotos",
                column: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductoFotos");
        }
    }
}
