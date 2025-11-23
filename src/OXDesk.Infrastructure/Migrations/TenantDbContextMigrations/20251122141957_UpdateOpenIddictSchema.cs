using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OXDesk.Infrastructure.Migrations.TenantDbContextMigrations
{
    /// <inheritdoc />
    public partial class UpdateOpenIddictSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "open_iddict_applications",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    application_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    client_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    client_secret = table.Column<string>(type: "text", nullable: true),
                    client_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    concurrency_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    consent_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    display_names = table.Column<string>(type: "text", nullable: true),
                    json_web_key_set = table.Column<string>(type: "text", nullable: true),
                    permissions = table.Column<string>(type: "text", nullable: true),
                    post_logout_redirect_uris = table.Column<string>(type: "text", nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    redirect_uris = table.Column<string>(type: "text", nullable: true),
                    requirements = table.Column<string>(type: "text", nullable: true),
                    settings = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_open_iddict_applications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "open_iddict_scopes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    concurrency_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    descriptions = table.Column<string>(type: "text", nullable: true),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    display_names = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    resources = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_open_iddict_scopes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "open_iddict_authorizations",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    application_id = table.Column<string>(type: "text", nullable: true),
                    concurrency_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    creation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    scopes = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_open_iddict_authorizations", x => x.id);
                    table.ForeignKey(
                        name: "FK_open_iddict_authorizations_open_iddict_applications_applica~",
                        column: x => x.application_id,
                        principalSchema: "public",
                        principalTable: "open_iddict_applications",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "open_iddict_tokens",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    application_id = table.Column<string>(type: "text", nullable: true),
                    authorization_id = table.Column<string>(type: "text", nullable: true),
                    concurrency_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    creation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expiration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    payload = table.Column<string>(type: "text", nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    redemption_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reference_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_open_iddict_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_open_iddict_tokens_open_iddict_applications_application_id",
                        column: x => x.application_id,
                        principalSchema: "public",
                        principalTable: "open_iddict_applications",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_open_iddict_tokens_open_iddict_authorizations_authorization~",
                        column: x => x.authorization_id,
                        principalSchema: "public",
                        principalTable: "open_iddict_authorizations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_applications_client_id",
                schema: "public",
                table: "open_iddict_applications",
                column: "client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_authorizations_application_id_status_subject_ty~",
                schema: "public",
                table: "open_iddict_authorizations",
                columns: new[] { "application_id", "status", "subject", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_scopes_name",
                schema: "public",
                table: "open_iddict_scopes",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_tokens_application_id_status_subject_type",
                schema: "public",
                table: "open_iddict_tokens",
                columns: new[] { "application_id", "status", "subject", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_tokens_authorization_id",
                schema: "public",
                table: "open_iddict_tokens",
                column: "authorization_id");

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_tokens_reference_id",
                schema: "public",
                table: "open_iddict_tokens",
                column: "reference_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "open_iddict_scopes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "open_iddict_tokens",
                schema: "public");

            migrationBuilder.DropTable(
                name: "open_iddict_authorizations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "open_iddict_applications",
                schema: "public");
        }
    }
}
