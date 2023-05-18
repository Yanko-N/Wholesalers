using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrpcService.Migrations
{
    /// <inheritdoc />
    public partial class TP2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isAdmin",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Apartamento",
                table: "Coberturas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Coberturas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "ACTIVE");

            migrationBuilder.CreateTable(
                name: "OperatorActionEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OperatorUsername = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CoberturaId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorActionEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperatorActionEvents_Coberturas_CoberturaId",
                        column: x => x.CoberturaId,
                        principalTable: "Coberturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OperatorActionEvents_Users_OperatorUsername",
                        column: x => x.OperatorUsername,
                        principalTable: "Users",
                        principalColumn: "Username",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UIDS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperatorUsername = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CoberturaId = table.Column<int>(type: "int", nullable: false),
                    UID = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UIDS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UIDS_Coberturas_CoberturaId",
                        column: x => x.CoberturaId,
                        principalTable: "Coberturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UIDS_Users_OperatorUsername",
                        column: x => x.OperatorUsername,
                        principalTable: "Users",
                        principalColumn: "Username",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OperatorActionEvents_CoberturaId",
                table: "OperatorActionEvents",
                column: "CoberturaId");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorActionEvents_OperatorUsername",
                table: "OperatorActionEvents",
                column: "OperatorUsername");

            migrationBuilder.CreateIndex(
                name: "IX_UIDS_CoberturaId",
                table: "UIDS",
                column: "CoberturaId");

            migrationBuilder.CreateIndex(
                name: "IX_UIDS_OperatorUsername",
                table: "UIDS",
                column: "OperatorUsername");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperatorActionEvents");

            migrationBuilder.DropTable(
                name: "UIDS");

            migrationBuilder.DropColumn(
                name: "isAdmin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Coberturas");

            migrationBuilder.AlterColumn<string>(
                name: "Apartamento",
                table: "Coberturas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
