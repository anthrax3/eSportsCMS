using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace WebDevEsports.Data.Migrations
{
    public partial class AnnouncementImage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Comment",
                newName: "Id");

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "Announcement",
                type: "varbinary(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Announcement");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Comment",
                newName: "ID");
        }
    }
}
