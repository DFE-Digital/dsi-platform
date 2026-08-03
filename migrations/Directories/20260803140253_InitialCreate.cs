using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace migrations.Directories
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "invitation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    email = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    code = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    firstName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    lastName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    originClientId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    originRedirectUri = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: true),
                    selfStarted = table.Column<bool>(type: "bit", nullable: false),
                    overrideSubject = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    overrideBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    previousUsername = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    previousPassword = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    previousSalt = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    deactivated = table.Column<bool>(type: "bit", nullable: false),
                    reason = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    completed = table.Column<bool>(type: "bit", nullable: false),
                    uid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    isMigrated = table.Column<bool>(type: "bit", nullable: false),
                    isApprover = table.Column<bool>(type: "bit", nullable: false),
                    approverEmail = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    orgName = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    codeMetaData = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invitation", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "password_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    password = table.Column<string>(type: "varchar(5000)", unicode: false, maxLength: 5000, nullable: false),
                    salt = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_password_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    sub = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    given_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    family_name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    password = table.Column<string>(type: "varchar(5000)", unicode: false, maxLength: 5000, nullable: false),
                    salt = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    phone_number = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    last_login = table.Column<DateTime>(type: "datetime", nullable: true),
                    isMigrated = table.Column<bool>(type: "bit", nullable: false),
                    job_title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    password_reset_required = table.Column<bool>(type: "bit", nullable: false),
                    prev_login = table.Column<DateTime>(type: "datetime", nullable: true),
                    is_entra = table.Column<bool>(type: "bit", nullable: false),
                    entra_oid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    entra_linked = table.Column<DateTime>(type: "datetime", nullable: true),
                    is_internal_user = table.Column<bool>(type: "bit", nullable: false),
                    entra_defer_until = table.Column<DateTime>(type: "datetime", nullable: true),
                    is_test_user = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__user__DDDF3AD9CA56D5BF", x => x.sub);
                });

            migrationBuilder.CreateTable(
                name: "user_code",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    codeType = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false, defaultValue: "PasswordReset"),
                    code = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    redirectUri = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    clientId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    email = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    contextData = table.Column<string>(type: "varchar(5000)", unicode: false, maxLength: 5000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__user_cod__E4DCEC89DE598738", x => new { x.uid, x.codeType });
                });

            migrationBuilder.CreateTable(
                name: "user_password_history",
                columns: table => new
                {
                    passwordHistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    userSub = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ck_user_password_history", x => new { x.passwordHistoryId, x.userSub });
                });

            migrationBuilder.CreateTable(
                name: "invitation_callback",
                columns: table => new
                {
                    invitationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    sourceId = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    callbackUrl = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    clientId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvitationCallback", x => new { x.invitationId, x.sourceId });
                    table.ForeignKey(
                        name: "FK_InvitationCallback_Invitation",
                        column: x => x.invitationId,
                        principalTable: "invitation",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_password_policy",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    uid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    policyCode = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    password_history_limit = table.Column<short>(type: "smallint", nullable: true, defaultValue: (short)3)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPasswordPolicy", x => x.id);
                    table.ForeignKey(
                        name: "FK__user_passwo__uid__72E607DB",
                        column: x => x.uid,
                        principalTable: "user",
                        principalColumn: "sub",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_status_change_reasons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    old_status = table.Column<short>(type: "smallint", nullable: false),
                    new_status = table.Column<short>(type: "smallint", nullable: false),
                    reason = table.Column<string>(type: "varchar(5000)", unicode: false, maxLength: 5000, nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStatusChangeReasons", x => new { x.id, x.user_id });
                    table.ForeignKey(
                        name: "FK_UserStatusChangeReasons_User",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "sub");
                });

            migrationBuilder.CreateIndex(
                name: "IDX__user__entra_oid__unique",
                table: "user",
                column: "entra_oid",
                unique: true,
                filter: "([entra_oid] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "idx_user_code_email",
                table: "user_code",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_user_password_policy_uid",
                table: "user_password_policy",
                column: "uid");

            migrationBuilder.CreateIndex(
                name: "IX_user_status_change_reasons_user_id",
                table: "user_status_change_reasons",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invitation_callback");

            migrationBuilder.DropTable(
                name: "password_history");

            migrationBuilder.DropTable(
                name: "user_code");

            migrationBuilder.DropTable(
                name: "user_password_history");

            migrationBuilder.DropTable(
                name: "user_password_policy");

            migrationBuilder.DropTable(
                name: "user_status_change_reasons");

            migrationBuilder.DropTable(
                name: "invitation");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}
