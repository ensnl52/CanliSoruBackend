using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanliSoruBackend.Migrations
{
    
    public partial class OyunBaslangicZamani : Migration
    {
       

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Kullainiciİd",
                table: "OyunCevaplari",
                newName: "Kullaniciİd");

            migrationBuilder.AddColumn<DateTime>(
                name: "BaslangicZamani",
                table: "Oyunlar",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaslangicZamani",
                table: "Oyunlar");

            migrationBuilder.RenameColumn(
                name: "Kullaniciİd",
                table: "OyunCevaplari",
                newName: "Kullainiciİd");
        }
    }
}
