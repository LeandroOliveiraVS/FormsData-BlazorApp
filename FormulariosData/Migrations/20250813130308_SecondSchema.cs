using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormulariosData.Migrations
{
    /// <inheritdoc />
    public partial class SecondSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CheckListVeiculos",
                columns: table => new
                {
                    IdCheckListVeiculo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Registro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NomeRecebedor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmailRecebedor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmailResponsavel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NomeResponsavel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NumeroVT = table.Column<int>(type: "int", nullable: false),
                    PlacaVeiculo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    KmVeiculo = table.Column<int>(type: "int", nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    Hora = table.Column<TimeOnly>(type: "time", nullable: false),
                    ChecagemFarol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChecagemLuzesIntermintentes = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChecagemLanternas = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChecagemLuzesInternas = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChecagemSirenes = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChecagemAcessorios = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Observacoes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelatoAvarias = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PastaImagens = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Relatorio = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckListVeiculos", x => x.IdCheckListVeiculo);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckListVeiculos");
        }
    }
}
