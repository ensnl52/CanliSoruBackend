using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanliSoruBackend.Migrations
{
    /// <inheritdoc />
    public partial class OyunOyuncuEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OdaKodu",
                table: "Oyunlar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "OyunOyunculari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OyunId = table.Column<int>(type: "int", nullable: false),
                    KullaniciId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Puan = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OyunOyunculari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OyunOyunculari_Oyunlar_OyunId",
                        column: x => x.OyunId,
                        principalTable: "Oyunlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OyunOyunculari_OyunId",
                table: "OyunOyunculari",
                column: "OyunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OyunOyunculari");

            migrationBuilder.DropColumn(
                name: "OdaKodu",
                table: "Oyunlar");
        }
    }
}
