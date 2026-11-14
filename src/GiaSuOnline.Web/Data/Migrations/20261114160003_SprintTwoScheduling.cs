using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiaSuOnline.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SprintTwoScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TutorSubjects_SubjectId",
                table: "TutorSubjects");

            migrationBuilder.DropIndex(
                name: "IX_TutorReviews_TutorProfileId",
                table: "TutorReviews");

            migrationBuilder.DropIndex(
                name: "IX_AvailabilitySlots_TutorProfileId",
                table: "AvailabilitySlots");

            migrationBuilder.AddColumn<long>(
                name: "ScheduleRevision",
                table: "TutorProfiles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "TeachingLanguage",
                table: "TutorProfiles",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "vi");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AvailabilitySlots",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Revision",
                table: "AvailabilitySlots",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE [AvailabilitySlots] SET [Revision] = REPLACE(CONVERT(varchar(36), NEWID()), '-', '') WHERE [Revision] = '';");

            migrationBuilder.CreateTable(
                name: "BookingEmails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Recipient = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextAttemptAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeaseUntilUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeaseToken = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingEmails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlotId = table.Column<int>(type: "int", nullable: false),
                    TutorProfileId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Revision = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_AvailabilitySlots_SlotId",
                        column: x => x.SlotId,
                        principalTable: "AvailabilitySlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_TutorProfiles_TutorProfileId",
                        column: x => x.TutorProfileId,
                        principalTable: "TutorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TutorSubjects_SubjectId_TutorProfileId",
                table: "TutorSubjects",
                columns: new[] { "SubjectId", "TutorProfileId" });

            migrationBuilder.CreateIndex(
                name: "IX_TutorReviews_TutorProfileId_CreatedAtUtc",
                table: "TutorReviews",
                columns: new[] { "TutorProfileId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TutorProfiles_Status_TeachingLanguage_HourlyRate",
                table: "TutorProfiles",
                columns: new[] { "Status", "TeachingLanguage", "HourlyRate" });

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilitySlots_TutorProfileId_StartsAtUtc_EndsAtUtc",
                table: "AvailabilitySlots",
                columns: new[] { "TutorProfileId", "StartsAtUtc", "EndsAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingEmails_SentAtUtc_NextAttemptAtUtc",
                table: "BookingEmails",
                columns: new[] { "SentAtUtc", "NextAttemptAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SlotId",
                table: "Bookings",
                column: "SlotId",
                unique: true,
                filter: "[Status] IN (0, 1)");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Status_ExpiresAtUtc",
                table: "Bookings",
                columns: new[] { "Status", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_StudentId_CreatedAtUtc",
                table: "Bookings",
                columns: new[] { "StudentId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TutorProfileId",
                table: "Bookings",
                column: "TutorProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingEmails");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_TutorSubjects_SubjectId_TutorProfileId",
                table: "TutorSubjects");

            migrationBuilder.DropIndex(
                name: "IX_TutorReviews_TutorProfileId_CreatedAtUtc",
                table: "TutorReviews");

            migrationBuilder.DropIndex(
                name: "IX_TutorProfiles_Status_TeachingLanguage_HourlyRate",
                table: "TutorProfiles");

            migrationBuilder.DropIndex(
                name: "IX_AvailabilitySlots_TutorProfileId_StartsAtUtc_EndsAtUtc",
                table: "AvailabilitySlots");

            migrationBuilder.DropColumn(
                name: "ScheduleRevision",
                table: "TutorProfiles");

            migrationBuilder.DropColumn(
                name: "TeachingLanguage",
                table: "TutorProfiles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AvailabilitySlots");

            migrationBuilder.DropColumn(
                name: "Revision",
                table: "AvailabilitySlots");

            migrationBuilder.CreateIndex(
                name: "IX_TutorSubjects_SubjectId",
                table: "TutorSubjects",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorReviews_TutorProfileId",
                table: "TutorReviews",
                column: "TutorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilitySlots_TutorProfileId",
                table: "AvailabilitySlots",
                column: "TutorProfileId");
        }
    }
}
