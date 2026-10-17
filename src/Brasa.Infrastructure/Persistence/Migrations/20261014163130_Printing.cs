using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Brasa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Printing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "printing");

            migrationBuilder.CreateTable(
                name: "agents",
                schema: "printing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    agent_version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    secret_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    paired_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_online = table.Column<bool>(type: "boolean", nullable: false),
                    local_queue_size = table.Column<int>(type: "integer", nullable: false),
                    last_error = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pairing_codes",
                schema: "printing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pairing_codes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "printers",
                schema: "printing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    paper_width = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status_message = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status_reported_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_printers", x => x.id);
                    table.ForeignKey(
                        name: "fk_printers_agents_agent_id",
                        column: x => x.agent_id,
                        principalSchema: "printing",
                        principalTable: "agents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "jobs",
                schema: "printing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    printer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reason = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    document = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    possible_duplicate = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    next_attempt_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    leased_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_error = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_jobs", x => x.id);
                    table.ForeignKey(
                        name: "fk_jobs_printers_printer_id",
                        column: x => x.printer_id,
                        principalSchema: "printing",
                        principalTable: "printers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_jobs_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalSchema: "kitchen",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "attempts",
                schema: "printing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    finished_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    error = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    print_job_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attempts", x => x.id);
                    table.ForeignKey(
                        name: "fk_attempts_jobs_print_job_id",
                        column: x => x.print_job_id,
                        principalSchema: "printing",
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_attempts_print_job_id",
                schema: "printing",
                table: "attempts",
                column: "print_job_id");

            migrationBuilder.CreateIndex(
                name: "ix_jobs_printer_id",
                schema: "printing",
                table: "jobs",
                column: "printer_id");

            migrationBuilder.CreateIndex(
                name: "ix_jobs_status_next_attempt_at",
                schema: "printing",
                table: "jobs",
                columns: new[] { "status", "next_attempt_at" });

            migrationBuilder.CreateIndex(
                name: "ix_jobs_ticket_id_printer_id_reason",
                schema: "printing",
                table: "jobs",
                columns: new[] { "ticket_id", "printer_id", "reason" },
                unique: true,
                filter: "reason IN ('Automatic', 'CancellationNotice')");

            migrationBuilder.CreateIndex(
                name: "ix_pairing_codes_code_hash",
                schema: "printing",
                table: "pairing_codes",
                column: "code_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_printers_agent_id",
                schema: "printing",
                table: "printers",
                column: "agent_id");

            migrationBuilder.CreateIndex(
                name: "ix_printers_is_default",
                schema: "printing",
                table: "printers",
                column: "is_default",
                unique: true,
                filter: "is_default");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attempts",
                schema: "printing");

            migrationBuilder.DropTable(
                name: "pairing_codes",
                schema: "printing");

            migrationBuilder.DropTable(
                name: "jobs",
                schema: "printing");

            migrationBuilder.DropTable(
                name: "printers",
                schema: "printing");

            migrationBuilder.DropTable(
                name: "agents",
                schema: "printing");
        }
    }
}
