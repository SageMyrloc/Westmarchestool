using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Westmarchestool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerDrivenMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TownMapSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpeditionId = table.Column<int>(type: "int", nullable: false),
                    SubmittedByPlayerId = table.Column<int>(type: "int", nullable: false),
                    SubmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HexesSubmitted = table.Column<int>(type: "int", nullable: false),
                    HexesAccepted = table.Column<int>(type: "int", nullable: false),
                    HexesConflicted = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TownMapSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TownMapSubmissions_Expeditions_ExpeditionId",
                        column: x => x.ExpeditionId,
                        principalTable: "Expeditions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TownMapSubmissions_Users_SubmittedByPlayerId",
                        column: x => x.SubmittedByPlayerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MapConflicts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Q = table.Column<int>(type: "int", nullable: false),
                    R = table.Column<int>(type: "int", nullable: false),
                    SubmissionId = table.Column<int>(type: "int", nullable: false),
                    NewSubmitterId = table.Column<int>(type: "int", nullable: false),
                    ExistingSubmitterId = table.Column<int>(type: "int", nullable: true),
                    NewTerrainType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExistingTerrainType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapConflicts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MapConflicts_TownMapSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "TownMapSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MapConflicts_Users_ExistingSubmitterId",
                        column: x => x.ExistingSubmitterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MapConflicts_Users_NewSubmitterId",
                        column: x => x.NewSubmitterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConflictVotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConflictId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    VoteForNew = table.Column<bool>(type: "bit", nullable: false),
                    VotedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConflictVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConflictVotes_MapConflicts_ConflictId",
                        column: x => x.ConflictId,
                        principalTable: "MapConflicts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConflictVotes_Users_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConflictVotes_ConflictId_PlayerId",
                table: "ConflictVotes",
                columns: new[] { "ConflictId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConflictVotes_PlayerId",
                table: "ConflictVotes",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MapConflicts_ExistingSubmitterId",
                table: "MapConflicts",
                column: "ExistingSubmitterId");

            migrationBuilder.CreateIndex(
                name: "IX_MapConflicts_NewSubmitterId",
                table: "MapConflicts",
                column: "NewSubmitterId");

            migrationBuilder.CreateIndex(
                name: "IX_MapConflicts_Q_R",
                table: "MapConflicts",
                columns: new[] { "Q", "R" });

            migrationBuilder.CreateIndex(
                name: "IX_MapConflicts_Status",
                table: "MapConflicts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MapConflicts_SubmissionId",
                table: "MapConflicts",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_TownMapSubmissions_ExpeditionId",
                table: "TownMapSubmissions",
                column: "ExpeditionId");

            migrationBuilder.CreateIndex(
                name: "IX_TownMapSubmissions_Status",
                table: "TownMapSubmissions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TownMapSubmissions_SubmittedByPlayerId",
                table: "TownMapSubmissions",
                column: "SubmittedByPlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConflictVotes");

            migrationBuilder.DropTable(
                name: "MapConflicts");

            migrationBuilder.DropTable(
                name: "TownMapSubmissions");
        }
    }
}
