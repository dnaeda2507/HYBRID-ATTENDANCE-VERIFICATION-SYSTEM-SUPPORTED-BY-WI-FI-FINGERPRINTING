using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations.Wifi
{
    /// <inheritdoc />
    public partial class AddAttendanceWifiColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "WifiConfidenceScore",
                table: "Attendances",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsWifiVerified",
                table: "Attendances",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WifiConfidenceScore",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "IsWifiVerified",
                table: "Attendances");
        }
    }
}
