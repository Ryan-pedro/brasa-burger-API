using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Brasa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PixOnlinePayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "accepts_online_pix",
                schema: "store",
                table: "stores",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "charge_id",
                schema: "payments",
                table: "payments",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "expires_at",
                schema: "payments",
                table: "payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pix_copy_paste",
                schema: "payments",
                table: "payments",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provider",
                schema: "payments",
                table: "payments",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "webhook_events",
                schema: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    event_id = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    charge_id = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    last_error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_webhook_events", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_payments_method_status",
                schema: "payments",
                table: "payments",
                columns: new[] { "method", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_provider_charge_id",
                schema: "payments",
                table: "payments",
                columns: new[] { "provider", "charge_id" },
                unique: true,
                filter: "charge_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_webhook_events_provider_event_id",
                schema: "payments",
                table: "webhook_events",
                columns: new[] { "provider", "event_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_webhook_events_received_at",
                schema: "payments",
                table: "webhook_events",
                column: "received_at",
                filter: "processed_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "webhook_events",
                schema: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_method_status",
                schema: "payments",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_provider_charge_id",
                schema: "payments",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "accepts_online_pix",
                schema: "store",
                table: "stores");

            migrationBuilder.DropColumn(
                name: "charge_id",
                schema: "payments",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "expires_at",
                schema: "payments",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "pix_copy_paste",
                schema: "payments",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "provider",
                schema: "payments",
                table: "payments");
        }
    }
}
