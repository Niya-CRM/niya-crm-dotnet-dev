using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OXDesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TicketsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tickets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket_number = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    channel = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    language = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    brand = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    brand_text = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    product = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    product_text = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    subject = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    priority = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    priority_score = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    status = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    status_text = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    work_flow_status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    work_flow_status_text = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    is_escalated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_spam = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_auto_closed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    has_scheduled_reply = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_response_overdue = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_overdue = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_reopened = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    topic = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    sub_topic = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    request_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    skills = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    layout_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contact = table.Column<Guid>(type: "uuid", nullable: true),
                    contact_email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    contact_phone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    contact_mobile = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    contact_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    supplied_email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    supplied_company = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    supplied_phone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    account = table.Column<Guid>(type: "uuid", nullable: true),
                    account_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    sla_id = table.Column<long>(type: "bigint", nullable: true),
                    milestone_status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    owner = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    team = table.Column<Guid>(type: "uuid", nullable: true),
                    team_text = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    department = table.Column<Guid>(type: "uuid", nullable: true),
                    department_text = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    organisation = table.Column<Guid>(type: "uuid", nullable: true),
                    organisation_text = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    ai_sentiment = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    ai_tone = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    ai_summary = table.Column<string>(type: "text", nullable: true),
                    ai_topic = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    ai_subtopic = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    ai_language = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    opened_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    first_resolution_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    first_resolution_due_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sla_start_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sla_paused_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sla_breach_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    due_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    response_due_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    customer_responded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    on_hold_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    parent = table.Column<int>(type: "integer", nullable: true),
                    attachment_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    comment_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    task_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    thread_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tickets", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tickets_brand",
                table: "tickets",
                column: "brand");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_created_at",
                table: "tickets",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_deleted_at",
                table: "tickets",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_organisation",
                table: "tickets",
                column: "organisation");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_owner",
                table: "tickets",
                column: "owner");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_status",
                table: "tickets",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_status_created_at_due_at_deleted_at",
                table: "tickets",
                columns: new[] { "status", "created_at", "due_at", "deleted_at" });

            migrationBuilder.CreateIndex(
                name: "ix_tickets_team",
                table: "tickets",
                column: "team");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_ticket_number",
                table: "tickets",
                column: "ticket_number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tickets");
        }
    }
}
