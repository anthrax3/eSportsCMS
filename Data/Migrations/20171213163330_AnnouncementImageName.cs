using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace WebDevEsports.Data.Migrations
{
    public partial class AnnouncementImageName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Announcement");

            migrationBuilder.AddColumn<string>(
                name: "ImageName",
                table: "Announcement",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageName",
                table: "Announcement");

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "Announcement",
                nullable: true);
        }
    }
}
