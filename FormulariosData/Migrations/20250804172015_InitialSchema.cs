using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormulariosData.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Recebimentos",
                columns: table => new
                {
                    IdRecebimento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Registro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NomeRecebedor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmailRecebedor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LocalRecebimento = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NumeroNotaFiscal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomeFornecedor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DataRecebimento = table.Column<DateOnly>(type: "date", nullable: false),
                    HoraRecebimento = table.Column<TimeOnly>(type: "time", nullable: false),
                    Observacoes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoRecebimento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TitularChavePix = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ChavePix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CaminhoFotoNotaFiscal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CaminhoFotoDocumentos = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recebimentos", x => x.IdRecebimento);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Recebimentos");
        }
    }
}
