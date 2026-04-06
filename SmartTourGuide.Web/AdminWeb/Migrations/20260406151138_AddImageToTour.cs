using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddImageToTour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_POIs_Categories_CategoryId",
                table: "POIs");

            migrationBuilder.DropForeignKey(
                name: "FK_POIs_Locations_LocationId",
                table: "POIs");

            migrationBuilder.DropIndex(
                name: "IX_POIs_CategoryId",
                table: "POIs");

            migrationBuilder.DropIndex(
                name: "IX_POIs_LocationId",
                table: "POIs");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "POIs");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "POIs");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "POIs",
                newName: "OwnerUsername");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "POIs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "POIs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "POIs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionJa",
                table: "POIs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionKo",
                table: "POIs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionVi",
                table: "POIs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionZh",
                table: "POIs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "POIs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "POIs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TriggerRadius",
                table: "POIs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Audios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PoiId",
                table: "Audios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoiId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LanguageUsed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_ActivityLogs_POIs_PoiId",
                        column: x => x.PoiId,
                        principalTable: "POIs",
                        principalColumn: "PoiId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    AdminId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.AdminId);
                });

            migrationBuilder.CreateTable(
                name: "Tours",
                columns: table => new
                {
                    TourId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TourName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageSource = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tours", x => x.TourId);
                });

            migrationBuilder.CreateTable(
                name: "TourDetails",
                columns: table => new
                {
                    TourDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TourId = table.Column<int>(type: "int", nullable: false),
                    PoiId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourDetails", x => x.TourDetailId);
                    table.ForeignKey(
                        name: "FK_TourDetails_POIs_PoiId",
                        column: x => x.PoiId,
                        principalTable: "POIs",
                        principalColumn: "PoiId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TourDetails_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "TourId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Audios_PoiId",
                table: "Audios",
                column: "PoiId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_PoiId",
                table: "ActivityLogs",
                column: "PoiId");

            migrationBuilder.CreateIndex(
                name: "IX_TourDetails_PoiId",
                table: "TourDetails",
                column: "PoiId");

            migrationBuilder.CreateIndex(
                name: "IX_TourDetails_TourId",
                table: "TourDetails",
                column: "TourId");

            migrationBuilder.AddForeignKey(
                name: "FK_Audios_POIs_PoiId",
                table: "Audios",
                column: "PoiId",
                principalTable: "POIs",
                principalColumn: "PoiId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Audios_POIs_PoiId",
                table: "Audios");

            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "TourDetails");

            migrationBuilder.DropTable(
                name: "Tours");

            migrationBuilder.DropIndex(
                name: "IX_Audios_PoiId",
                table: "Audios");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "POIs");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "POIs");

            migrationBuilder.DropColumn(
                name: "DescriptionJa",
                table: "POIs");

            migrationBuilder.DropColumn(
                name: "DescriptionKo",
                table: "POIs");

            migrationBuilder.DropColumn(
                name: "DescriptionVi",
                table: "POIs");

            migrationBuilder.DropColumn(
                name: "DescriptionZh",
                table: "POIs");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "POIs");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "POIs");

            migrationBuilder.DropColumn(
                name: "TriggerRadius",
                table: "POIs");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Audios");

            migrationBuilder.DropColumn(
                name: "PoiId",
                table: "Audios");

            migrationBuilder.RenameColumn(
                name: "OwnerUsername",
                table: "POIs",
                newName: "Description");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "POIs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "POIs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "POIs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_POIs_CategoryId",
                table: "POIs",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_POIs_LocationId",
                table: "POIs",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_POIs_Categories_CategoryId",
                table: "POIs",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_POIs_Locations_LocationId",
                table: "POIs",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
