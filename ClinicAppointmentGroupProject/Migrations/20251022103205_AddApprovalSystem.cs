using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAppointmentGroupProject.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "aspnetusers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "aspnetusers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "aspnetusers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "aspnetusers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "aspnetusers");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "aspnetusers");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "aspnetusers");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "aspnetusers");
        }
    }
}
