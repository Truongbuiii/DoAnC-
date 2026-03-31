using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminWeb.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc /> 
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Audios_POIs_PoiId",
                table: "Audios");

            migrationBuilder.DropIndex(
                name: "IX_Audios_PoiId",
                table: "Audios");

            migrationBuilder.DropColumn(
                name: "PoiId",
                table: "Audios");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Audios",
                newName: "AudioName");

            migrationBuilder.RenameColumn(
                name: "Language",
                table: "Audios",
                newName: "FilePath");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "Audios",
                newName: "Description");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Audios",
                newName: "Language");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Audios",
                newName: "FileName");

            migrationBuilder.RenameColumn(
                name: "AudioName",
                table: "Audios",
                newName: "Title");

            migrationBuilder.AddColumn<int>(
                name: "PoiId",
                table: "Audios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Audios_PoiId",
                table: "Audios",
                column: "PoiId");

            migrationBuilder.AddForeignKey(
                name: "FK_Audios_POIs_PoiId",
                table: "Audios",
                column: "PoiId",
                principalTable: "POIs",
                principalColumn: "PoiId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
