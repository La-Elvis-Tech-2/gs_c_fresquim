using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fresquim.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PontosApoio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Horario = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TipoPonto = table.Column<int>(type: "INTEGER", nullable: false),
                    Bairro = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Logradouro = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    AguaGelada = table.Column<bool>(type: "INTEGER", nullable: true),
                    Linha = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true),
                    AtendeEmergencia = table.Column<bool>(type: "INTEGER", nullable: true),
                    CapacidadePessoas = table.Column<int>(type: "INTEGER", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PontosApoio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Avisos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 280, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PontoApoioId = table.Column<Guid>(type: "TEXT", nullable: true),
                    NomeLocalSugerido = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    RespostaPrefeitura = table.Column<string>(type: "TEXT", maxLength: 280, nullable: true),
                    ResolvidoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avisos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Avisos_PontosApoio_PontoApoioId",
                        column: x => x.PontoApoioId,
                        principalTable: "PontosApoio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Avisos_PontoApoioId",
                table: "Avisos",
                column: "PontoApoioId");

            migrationBuilder.CreateIndex(
                name: "IX_Avisos_Status",
                table: "Avisos",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PontosApoio_Nome",
                table: "PontosApoio",
                column: "Nome");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Avisos");

            migrationBuilder.DropTable(
                name: "PontosApoio");
        }
    }
}
