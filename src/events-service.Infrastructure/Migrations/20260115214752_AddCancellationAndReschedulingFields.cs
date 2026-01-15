using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eventsservice.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCancellationAndReschedulingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cancelado_por",
                table: "eventos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "contador_reprogramaciones",
                table: "eventos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_cancelacion",
                table: "eventos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_fin_original",
                table: "eventos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_inicio_original",
                table: "eventos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_cancelacion",
                table: "eventos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ultima_reprogramacion_fecha",
                table: "eventos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ultima_reprogramacion_por",
                table: "eventos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cancelado_por",
                table: "eventos");

            migrationBuilder.DropColumn(
                name: "contador_reprogramaciones",
                table: "eventos");

            migrationBuilder.DropColumn(
                name: "fecha_cancelacion",
                table: "eventos");

            migrationBuilder.DropColumn(
                name: "fecha_fin_original",
                table: "eventos");

            migrationBuilder.DropColumn(
                name: "fecha_inicio_original",
                table: "eventos");

            migrationBuilder.DropColumn(
                name: "motivo_cancelacion",
                table: "eventos");

            migrationBuilder.DropColumn(
                name: "ultima_reprogramacion_fecha",
                table: "eventos");

            migrationBuilder.DropColumn(
                name: "ultima_reprogramacion_por",
                table: "eventos");
        }
    }
}
