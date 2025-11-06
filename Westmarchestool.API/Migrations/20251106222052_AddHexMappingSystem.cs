using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Westmarchestool.API.Migrations
{
    /// <inheritdoc />
    public partial class AddHexMappingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Expeditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LeaderPlayerId = table.Column<int>(type: "int", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartQ = table.Column<int>(type: "int", nullable: false),
                    StartR = table.Column<int>(type: "int", nullable: false),
                    LastKnownQ = table.Column<int>(type: "int", nullable: true),
                    LastKnownR = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expeditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expeditions_Users_LeaderPlayerId",
                        column: x => x.LeaderPlayerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExpeditionHexes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpeditionId = table.Column<int>(type: "int", nullable: false),
                    Q = table.Column<int>(type: "int", nullable: false),
                    R = table.Column<int>(type: "int", nullable: false),
                    ActualQ = table.Column<int>(type: "int", nullable: false),
                    ActualR = table.Column<int>(type: "int", nullable: false),
                    TerrainType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsAccurate = table.Column<bool>(type: "bit", nullable: false),
                    ExploredTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpeditionHexes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpeditionHexes_Expeditions_ExpeditionId",
                        column: x => x.ExpeditionId,
                        principalTable: "Expeditions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpeditionMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpeditionId = table.Column<int>(type: "int", nullable: false),
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpeditionMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpeditionMembers_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpeditionMembers_Expeditions_ExpeditionId",
                        column: x => x.ExpeditionId,
                        principalTable: "Expeditions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpeditionMembers_Users_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HexTiles",
                columns: table => new
                {
                    Q = table.Column<int>(type: "int", nullable: false),
                    R = table.Column<int>(type: "int", nullable: false),
                    TerrainType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsExploredByGM = table.Column<bool>(type: "bit", nullable: false),
                    IsOnTownMap = table.Column<bool>(type: "bit", nullable: false),
                    DiscoveredByExpeditionId = table.Column<int>(type: "int", nullable: true),
                    DiscoveredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsAccurate = table.Column<bool>(type: "bit", nullable: false),
                    ActualQ = table.Column<int>(type: "int", nullable: true),
                    ActualR = table.Column<int>(type: "int", nullable: true),
                    GMNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasPOI = table.Column<bool>(type: "bit", nullable: false),
                    POIData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HexTiles", x => new { x.Q, x.R });
                    table.ForeignKey(
                        name: "FK_HexTiles_Expeditions_DiscoveredByExpeditionId",
                        column: x => x.DiscoveredByExpeditionId,
                        principalTable: "Expeditions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpeditionHexes_ExpeditionId_Q_R",
                table: "ExpeditionHexes",
                columns: new[] { "ExpeditionId", "Q", "R" });

            migrationBuilder.CreateIndex(
                name: "IX_ExpeditionMembers_CharacterId",
                table: "ExpeditionMembers",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpeditionMembers_ExpeditionId",
                table: "ExpeditionMembers",
                column: "ExpeditionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpeditionMembers_PlayerId",
                table: "ExpeditionMembers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Expeditions_LeaderPlayerId",
                table: "Expeditions",
                column: "LeaderPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Expeditions_Status",
                table: "Expeditions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_HexTiles_DiscoveredByExpeditionId",
                table: "HexTiles",
                column: "DiscoveredByExpeditionId");

            migrationBuilder.CreateIndex(
                name: "IX_HexTiles_IsExploredByGM",
                table: "HexTiles",
                column: "IsExploredByGM");

            migrationBuilder.CreateIndex(
                name: "IX_HexTiles_IsOnTownMap",
                table: "HexTiles",
                column: "IsOnTownMap");

            migrationBuilder.CreateIndex(
                name: "IX_HexTiles_TerrainType",
                table: "HexTiles",
                column: "TerrainType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpeditionHexes");

            migrationBuilder.DropTable(
                name: "ExpeditionMembers");

            migrationBuilder.DropTable(
                name: "HexTiles");

            migrationBuilder.DropTable(
                name: "Expeditions");
        }
    }
}
