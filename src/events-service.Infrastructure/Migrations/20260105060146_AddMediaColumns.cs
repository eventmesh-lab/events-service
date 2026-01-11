using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eventsservice.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "folleto_blob",
                table: "eventos",
                type: "character varying(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "folleto_content_type",
                table: "eventos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "folleto_size_bytes",
                table: "eventos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "imagen_principal_blob",
                table: "eventos",
                type: "character varying(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "imagen_principal_content_type",
                table: "eventos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "imagen_principal_size_bytes",
                table: "eventos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "imagenes_secundarias_json",
                table: "eventos",
                type: "text",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "folleto_blob",
                table: "eventos");

            migrationBuilder.DropColumn(
                name: "folleto_content_type",
                table: "eventos");

            migrationBuilder.DropColumn(
                name: "folleto_size_bytes",
                table: "eventos");

            migrationBuilder.DropColumn(
                name: "imagen_principal_blob",
                table: "eventos");

            migrationBuilder.DropColumn(
                name: "imagen_principal_content_type",
                table: "eventos");

            migrationBuilder.DropColumn(
                name: "imagen_principal_size_bytes",
                table: "eventos");

            migrationBuilder.DropColumn(
                name: "imagenes_secundarias_json",
                table: "eventos");
        }
    }
}
