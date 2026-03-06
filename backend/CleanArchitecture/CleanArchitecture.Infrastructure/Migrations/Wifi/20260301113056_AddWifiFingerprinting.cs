using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations.Wifi
{
    /// <inheritdoc />
    public partial class AddWifiFingerprinting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "MinimumConfidence",
                table: "Sessions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "WifiVerificationRequired",
                table: "Sessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ClassroomId",
                table: "Courses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Classrooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Building = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classrooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentWifiScans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    ScannedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PredictedClassroomId = table.Column<int>(type: "int", nullable: true),
                    ConfidenceScore = table.Column<double>(type: "float", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentWifiScans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentWifiScans_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentWifiScans_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WifiTrainingSamples",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassroomId = table.Column<int>(type: "int", nullable: false),
                    CollectedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WifiTrainingSamples", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WifiTrainingSamples_Classrooms_ClassroomId",
                        column: x => x.ClassroomId,
                        principalTable: "Classrooms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StudentWifiAccessPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentWifiScanId = table.Column<int>(type: "int", nullable: false),
                    Bssid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rssi = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentWifiAccessPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentWifiAccessPoints_StudentWifiScans_StudentWifiScanId",
                        column: x => x.StudentWifiScanId,
                        principalTable: "StudentWifiScans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WifiTrainingAccessPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WifiTrainingSampleId = table.Column<int>(type: "int", nullable: false),
                    Bssid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rssi = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WifiTrainingAccessPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WifiTrainingAccessPoints_WifiTrainingSamples_WifiTrainingSampleId",
                        column: x => x.WifiTrainingSampleId,
                        principalTable: "WifiTrainingSamples",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_ClassroomId",
                table: "Courses",
                column: "ClassroomId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentWifiAccessPoints_StudentWifiScanId",
                table: "StudentWifiAccessPoints",
                column: "StudentWifiScanId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentWifiScans_SessionId",
                table: "StudentWifiScans",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentWifiScans_StudentId",
                table: "StudentWifiScans",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_WifiTrainingAccessPoints_WifiTrainingSampleId",
                table: "WifiTrainingAccessPoints",
                column: "WifiTrainingSampleId");

            migrationBuilder.CreateIndex(
                name: "IX_WifiTrainingSamples_ClassroomId",
                table: "WifiTrainingSamples",
                column: "ClassroomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Classrooms_ClassroomId",
                table: "Courses",
                column: "ClassroomId",
                principalTable: "Classrooms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Classrooms_ClassroomId",
                table: "Courses");

            migrationBuilder.DropTable(
                name: "StudentWifiAccessPoints");

            migrationBuilder.DropTable(
                name: "WifiTrainingAccessPoints");

            migrationBuilder.DropTable(
                name: "StudentWifiScans");

            migrationBuilder.DropTable(
                name: "WifiTrainingSamples");

            migrationBuilder.DropTable(
                name: "Classrooms");

            migrationBuilder.DropIndex(
                name: "IX_Courses_ClassroomId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "MinimumConfidence",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "WifiVerificationRequired",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "ClassroomId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "IsWifiVerified",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "WifiConfidenceScore",
                table: "Attendances");
        }
    }
}
