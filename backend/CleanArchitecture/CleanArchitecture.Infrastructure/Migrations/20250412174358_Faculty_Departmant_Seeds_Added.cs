using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Faculty_Departmant_Seeds_Added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Faculties",
                columns: new[] { "Id", "Created", "CreatedBy", "Description", "LastModified", "LastModifiedBy", "Name" },
                values: new object[] { 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Engineering Faculty", null, null, "Engineering" });

            migrationBuilder.InsertData(
                table: "Departmants",
                columns: new[] { "Id", "Created", "CreatedBy", "Description", "FacultyId", "LastModified", "LastModifiedBy", "Name" },
                values: new object[] { 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Software Engineering Departmant", 1, null, null, "Software Engineering" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Departmants",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Faculties",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
