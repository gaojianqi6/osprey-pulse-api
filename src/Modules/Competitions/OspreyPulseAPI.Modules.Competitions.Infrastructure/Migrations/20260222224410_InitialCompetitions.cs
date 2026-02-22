using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OspreyPulseAPI.Modules.Competitions.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCompetitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "competitions");

            migrationBuilder.CreateTable(
                name: "Channels",
                schema: "competitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Slug = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Channels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                schema: "competitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Nationality = table.Column<string>(type: "text", nullable: true),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true),
                    DefaultPosition = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Leagues",
                schema: "competitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    LogoUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leagues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leagues_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalSchema: "competitions",
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                schema: "competitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LeagueId = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seasons_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalSchema: "competitions",
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                schema: "competitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LeagueId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalSchema: "competitions",
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "competitions",
                schema: "competitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SeasonId = table.Column<int>(type: "integer", nullable: false),
                    HomeTeamId = table.Column<int>(type: "integer", nullable: false),
                    AwayTeamId = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    HomeScore = table.Column<int>(type: "integer", nullable: false),
                    AwayScore = table.Column<int>(type: "integer", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_competitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_competitions_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalSchema: "competitions",
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_competitions_Teams_AwayTeamId",
                        column: x => x.AwayTeamId,
                        principalSchema: "competitions",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_competitions_Teams_HomeTeamId",
                        column: x => x.HomeTeamId,
                        principalSchema: "competitions",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "player_team_assignments",
                schema: "competitions",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    TeamId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    JoinedDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_team_assignments", x => new { x.PlayerId, x.TeamId });
                    table.ForeignKey(
                        name: "FK_player_team_assignments_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "competitions",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_player_team_assignments_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "competitions",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "competition_rosters",
                schema: "competitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompetitionId = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<int>(type: "integer", nullable: true),
                    TeamId = table.Column<int>(type: "integer", nullable: false),
                    RoleType = table.Column<short>(type: "smallint", nullable: true),
                    LineupStatus = table.Column<short>(type: "smallint", nullable: true),
                    TeamSide = table.Column<short>(type: "smallint", nullable: true),
                    stats = table.Column<string>(type: "jsonb", nullable: true),
                    RatingAvg = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_competition_rosters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_competition_rosters_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "competitions",
                        principalTable: "Players",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_competition_rosters_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "competitions",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_competition_rosters_competitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalSchema: "competitions",
                        principalTable: "competitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_competition_rosters_CompetitionId",
                schema: "competitions",
                table: "competition_rosters",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_competition_rosters_PlayerId",
                schema: "competitions",
                table: "competition_rosters",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_competition_rosters_TeamId",
                schema: "competitions",
                table: "competition_rosters",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_competitions_AwayTeamId",
                schema: "competitions",
                table: "competitions",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_competitions_HomeTeamId",
                schema: "competitions",
                table: "competitions",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_competitions_SeasonId",
                schema: "competitions",
                table: "competitions",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_ChannelId",
                schema: "competitions",
                table: "Leagues",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_player_team_assignments_TeamId",
                schema: "competitions",
                table: "player_team_assignments",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_LeagueId",
                schema: "competitions",
                table: "Seasons",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_LeagueId",
                schema: "competitions",
                table: "Teams",
                column: "LeagueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "competition_rosters",
                schema: "competitions");

            migrationBuilder.DropTable(
                name: "player_team_assignments",
                schema: "competitions");

            migrationBuilder.DropTable(
                name: "competitions",
                schema: "competitions");

            migrationBuilder.DropTable(
                name: "Players",
                schema: "competitions");

            migrationBuilder.DropTable(
                name: "Seasons",
                schema: "competitions");

            migrationBuilder.DropTable(
                name: "Teams",
                schema: "competitions");

            migrationBuilder.DropTable(
                name: "Leagues",
                schema: "competitions");

            migrationBuilder.DropTable(
                name: "Channels",
                schema: "competitions");
        }
    }
}
