using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OXDesk.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "app_installation_status",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10001', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pipeline = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    version = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    step = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    completed = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_installation_status", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    object_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    @event = table.Column<string>(name: "event", type: "character varying(100)", maxLength: 100, nullable: false),
                    object_item_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    data = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "brands",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10001', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    brand_name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    logo = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    logo_dark = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    brand_color = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    website = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_brands", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "change_history_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    object_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    object_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    field_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    old_value = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    new_value = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_change_history_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "channels",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10001', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    channel_name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_channels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dynamic_object_field_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10001', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    field_type_key = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    min_length = table.Column<int>(type: "integer", nullable: true),
                    max_length = table.Column<int>(type: "integer", nullable: true),
                    decimals = table.Column<int>(type: "integer", nullable: true),
                    max_file_size = table.Column<int>(type: "integer", nullable: true),
                    allowed_file_types = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    min_file_count = table.Column<int>(type: "integer", nullable: true),
                    max_file_count = table.Column<int>(type: "integer", nullable: true),
                    value_list_id = table.Column<Guid>(type: "uuid", nullable: true),
                    min_selected_items = table.Column<int>(type: "integer", nullable: true),
                    max_selected_items = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dynamic_object_field_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dynamic_object_fields",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10001', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    object_id = table.Column<int>(type: "integer", nullable: false),
                    field_type_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    indexed = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    help_text = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    placeholder = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true),
                    required = table.Column<bool>(type: "boolean", nullable: false),
                    unique = table.Column<bool>(type: "boolean", nullable: false),
                    min_length = table.Column<int>(type: "integer", nullable: true),
                    max_length = table.Column<int>(type: "integer", nullable: true),
                    decimals = table.Column<int>(type: "integer", nullable: true),
                    max_file_size = table.Column<int>(type: "integer", nullable: true),
                    allowed_file_types = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    min_file_count = table.Column<int>(type: "integer", nullable: true),
                    max_file_count = table.Column<int>(type: "integer", nullable: true),
                    value_list_id = table.Column<Guid>(type: "uuid", nullable: true),
                    min_selected_items = table.Column<int>(type: "integer", nullable: true),
                    max_selected_items = table.Column<int>(type: "integer", nullable: true),
                    editable = table.Column<bool>(type: "boolean", nullable: false),
                    visible_on_create = table.Column<bool>(type: "boolean", nullable: false),
                    visible_on_edit = table.Column<bool>(type: "boolean", nullable: false),
                    visible_on_view = table.Column<bool>(type: "boolean", nullable: false),
                    audit_changes = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dynamic_object_fields", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dynamic_objects",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10001', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    object_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    singular_name = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    plural_name = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    object_key = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    object_type = table.Column<string>(type: "varchar(10)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dynamic_objects", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "priorities",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10001', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    priority_name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    increment_score = table.Column<int>(type: "integer", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_priorities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "statuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10001', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    object_id = table.Column<int>(type: "integer", nullable: false),
                    status_name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    status_type = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    host = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    time_zone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    database_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<string>(type: "text", nullable: false, defaultValue: "True"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", maxLength: 100, nullable: false),
                    last_modified_by = table.Column<Guid>(type: "uuid", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tickets",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000001', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket_number = table.Column<int>(type: "integer", nullable: false),
                    channel_id = table.Column<int>(type: "integer", nullable: false),
                    channel_name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    language = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    brand_id = table.Column<int>(type: "integer", nullable: false),
                    brand_name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    product_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    subject = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    priority_id = table.Column<int>(type: "integer", nullable: false),
                    priority_name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    priority_score = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    status_id = table.Column<int>(type: "integer", nullable: false),
                    status_name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    status_type = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    work_flow_id = table.Column<int>(type: "integer", nullable: true),
                    work_flow_status_id = table.Column<int>(type: "integer", nullable: true),
                    work_flow_status_name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
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
                    account_id = table.Column<int>(type: "integer", nullable: true),
                    account_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    sla_id = table.Column<long>(type: "bigint", nullable: true),
                    milestone_status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    owner_id = table.Column<int>(type: "integer", nullable: true),
                    owner_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    team_id = table.Column<int>(type: "integer", nullable: true),
                    team_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    department_id = table.Column<int>(type: "integer", nullable: true),
                    department_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    organisation_id = table.Column<int>(type: "integer", nullable: true),
                    organisation_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
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
                    parent_ticket_number = table.Column<int>(type: "integer", nullable: true),
                    attachment_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    comment_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    task_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    thread_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tickets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    last_name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    location = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false, defaultValue: ""),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    profile = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    time_zone = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "value_lists",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10001', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    list_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    list_key = table.Column<string>(type: "varchar(50)", maxLength: 60, nullable: false),
                    description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    value_list_type = table.Column<string>(type: "varchar(10)", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    allow_modify = table.Column<bool>(type: "boolean", nullable: false),
                    allow_new_item = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_value_lists", x => x.id);
                    table.UniqueConstraint("ak_value_lists_list_key", x => x.list_key);
                });

            migrationBuilder.CreateTable(
                name: "workflow_mappings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10001', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    work_flow_id = table.Column<int>(type: "integer", nullable: false),
                    topic_id = table.Column<int>(type: "integer", nullable: true),
                    sub_topic_id = table.Column<int>(type: "integer", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workflow_mappings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "workflow_statuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10001', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    work_flow_id = table.Column<int>(type: "integer", nullable: false),
                    status_id = table.Column<int>(type: "integer", nullable: false),
                    work_flow_status_name = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workflow_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "workflows",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10001', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    work_flow_name = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workflows", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "FK_role_claims_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_claims_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_logins",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "FK_user_logins_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hashed_token = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    device = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    used_counter = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_tokens",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "FK_user_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "value_list_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10001', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    item_key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    list_key = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_value_list_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_value_list_items_value_lists_list_key",
                        column: x => x.list_key,
                        principalTable: "value_lists",
                        principalColumn: "list_key",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_app_installation_status_version",
                table: "app_installation_status",
                column: "version");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_tenant_id",
                table: "audit_logs",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_tenant_id_object_key_created_at",
                table: "audit_logs",
                columns: new[] { "tenant_id", "object_key", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_tenant_id_object_key_object_item_id_created_at",
                table: "audit_logs",
                columns: new[] { "tenant_id", "object_key", "object_item_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_brands_id",
                table: "brands",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_brands_tenant_id",
                table: "brands",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_brands_tenant_id_brand_id",
                table: "brands",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_change_history_logs_tenant_id_created_at",
                table: "change_history_logs",
                columns: new[] { "tenant_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_change_history_logs_tenant_id_object_key_object_item_id_cre~",
                table: "change_history_logs",
                columns: new[] { "tenant_id", "object_key", "object_item_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_channels_id",
                table: "channels",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_channels_tenant_id",
                table: "channels",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_channels_tenant_id_channel_id",
                table: "channels",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_dynamic_object_field_types_field_type_key",
                table: "dynamic_object_field_types",
                column: "field_type_key");

            migrationBuilder.CreateIndex(
                name: "ix_dynamic_object_fields_tenant_id",
                table: "dynamic_object_fields",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_dynamic_object_fields_tenant_id_object_id",
                table: "dynamic_object_fields",
                columns: new[] { "tenant_id", "object_id" });

            migrationBuilder.CreateIndex(
                name: "ix_dynamic_objects_tenant_id",
                table: "dynamic_objects",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_dynamic_objects_tenant_id_object_key",
                table: "dynamic_objects",
                columns: new[] { "tenant_id", "object_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_permissions_tenant_id",
                table: "permissions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_permissions_tenant_id_normalized_name",
                table: "permissions",
                columns: new[] { "tenant_id", "normalized_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_priorities_id",
                table: "priorities",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_priorities_tenant_id",
                table: "priorities",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_priorities_tenant_id_priority_id",
                table: "priorities",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_role_claims_role_id",
                table: "role_claims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_role_claims_tenant_id",
                table: "role_claims",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_role_claims_tenant_id_role_id",
                table: "role_claims",
                columns: new[] { "tenant_id", "role_id" });

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_roles_tenant_id",
                table: "roles",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_roles_tenant_id_normalized_name",
                table: "roles",
                columns: new[] { "tenant_id", "normalized_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "role_name_index",
                table: "roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_statuses_tenant_id",
                table: "statuses",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenants_host",
                table: "tenants",
                column: "host",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenants_is_active",
                table: "tenants",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_tenants_name",
                table: "tenants",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_tenant_id",
                table: "tickets",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_tenant_id_brand_id",
                table: "tickets",
                columns: new[] { "tenant_id", "brand_id" });

            migrationBuilder.CreateIndex(
                name: "ix_tickets_tenant_id_channel_id",
                table: "tickets",
                columns: new[] { "tenant_id", "channel_id" });

            migrationBuilder.CreateIndex(
                name: "ix_tickets_tenant_id_created_at",
                table: "tickets",
                columns: new[] { "tenant_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_tickets_tenant_id_deleted_at",
                table: "tickets",
                columns: new[] { "tenant_id", "deleted_at" });

            migrationBuilder.CreateIndex(
                name: "ix_tickets_tenant_id_organisation_id",
                table: "tickets",
                columns: new[] { "tenant_id", "organisation_id" });

            migrationBuilder.CreateIndex(
                name: "ix_tickets_tenant_id_owner_id",
                table: "tickets",
                columns: new[] { "tenant_id", "owner_id" });

            migrationBuilder.CreateIndex(
                name: "ix_tickets_tenant_id_status_created_due_deleted",
                table: "tickets",
                columns: new[] { "tenant_id", "status_id", "created_at", "due_at", "deleted_at" });

            migrationBuilder.CreateIndex(
                name: "ix_tickets_tenant_id_status_id",
                table: "tickets",
                columns: new[] { "tenant_id", "status_id" });

            migrationBuilder.CreateIndex(
                name: "ix_tickets_tenant_id_team_id",
                table: "tickets",
                columns: new[] { "tenant_id", "team_id" });

            migrationBuilder.CreateIndex(
                name: "ix_tickets_tenant_id_ticket_number",
                table: "tickets",
                columns: new[] { "tenant_id", "ticket_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_claims_tenant_id",
                table: "user_claims",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_claims_tenant_id_user_id",
                table: "user_claims",
                columns: new[] { "tenant_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "ix_user_claims_user_id",
                table: "user_claims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_logins_tenant_id",
                table: "user_logins",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_logins_tenant_id_login_provider_provider_key",
                table: "user_logins",
                columns: new[] { "tenant_id", "login_provider", "provider_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_logins_tenant_id_user_id",
                table: "user_logins",
                columns: new[] { "tenant_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "ix_user_logins_user_id",
                table: "user_logins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_refresh_tokens_expires_at",
                table: "user_refresh_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_refresh_tokens_used_at",
                table: "user_refresh_tokens",
                column: "used_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_refresh_tokens_user_id",
                table: "user_refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_tenant_id",
                table: "user_roles",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_tenant_id_user_id_role_id",
                table: "user_roles",
                columns: new[] { "tenant_id", "user_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_tokens_tenant_id",
                table: "user_tokens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_tokens_tenant_id_user_id",
                table: "user_tokens",
                columns: new[] { "tenant_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "ix_user_tokens_tenant_id_user_id_login_provider_name",
                table: "user_tokens",
                columns: new[] { "tenant_id", "user_id", "login_provider", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "email_index",
                table: "users",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_users_tenant_id",
                table: "users",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_users_tenant_id_normalized_email",
                table: "users",
                columns: new[] { "tenant_id", "normalized_email" });

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_users_tenant_id_normalized_user_name",
                table: "users",
                columns: new[] { "tenant_id", "normalized_user_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "user_name_index",
                table: "users",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_value_list_items_list_key",
                table: "value_list_items",
                column: "list_key");

            migrationBuilder.CreateIndex(
                name: "ix_value_list_items_tenant_id_list_key",
                table: "value_list_items",
                columns: new[] { "tenant_id", "list_key" });

            migrationBuilder.CreateIndex(
                name: "ix_value_lists_tenant_id",
                table: "value_lists",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_value_lists_tenant_id_list_key",
                table: "value_lists",
                columns: new[] { "tenant_id", "list_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workflow_mappings_tenant_id",
                table: "workflow_mappings",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ux_workflow_mappings_tenant_id_topic_id_sub_topic_id",
                table: "workflow_mappings",
                columns: new[] { "tenant_id", "topic_id", "sub_topic_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workflow_statuses_tenant_id",
                table: "workflow_statuses",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_workflows_tenant_id",
                table: "workflows",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_installation_status");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "brands");

            migrationBuilder.DropTable(
                name: "change_history_logs");

            migrationBuilder.DropTable(
                name: "channels");

            migrationBuilder.DropTable(
                name: "dynamic_object_field_types");

            migrationBuilder.DropTable(
                name: "dynamic_object_fields");

            migrationBuilder.DropTable(
                name: "dynamic_objects");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "priorities");

            migrationBuilder.DropTable(
                name: "role_claims");

            migrationBuilder.DropTable(
                name: "statuses");

            migrationBuilder.DropTable(
                name: "tenants");

            migrationBuilder.DropTable(
                name: "tickets");

            migrationBuilder.DropTable(
                name: "user_claims");

            migrationBuilder.DropTable(
                name: "user_logins");

            migrationBuilder.DropTable(
                name: "user_refresh_tokens");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "user_tokens");

            migrationBuilder.DropTable(
                name: "value_list_items");

            migrationBuilder.DropTable(
                name: "workflow_mappings");

            migrationBuilder.DropTable(
                name: "workflow_statuses");

            migrationBuilder.DropTable(
                name: "workflows");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "value_lists");
        }
    }
}
