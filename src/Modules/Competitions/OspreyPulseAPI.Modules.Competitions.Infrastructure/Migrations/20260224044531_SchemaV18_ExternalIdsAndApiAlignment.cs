using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OspreyPulseAPI.Modules.Competitions.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SchemaV18_ExternalIdsAndApiAlignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Seasons_LeagueId",
                schema: "competitions",
                table: "Seasons");

            migrationBuilder.DropIndex(
                name: "IX_competitions_SeasonId",
                schema: "competitions",
                table: "competitions");

            migrationBuilder.DropColumn(
                name: "TeamSide",
                schema: "competitions",
                table: "competition_rosters");

            // Map old CompetitionStatus enum (0-3) to API-aligned (1-6): 0->1, 1->2, 2->3, 3->5
            migrationBuilder.Sql(@"
                UPDATE competitions.competitions
                SET ""Status"" = CASE ""Status""
                    WHEN 0 THEN 1
                    WHEN 1 THEN 2
                    WHEN 2 THEN 3
                    WHEN 3 THEN 5
                    ELSE 1
                END;");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "competitions",
                table: "Teams",
                newName: "Nickname");

            migrationBuilder.RenameColumn(
                name: "Label",
                schema: "competitions",
                table: "Seasons",
                newName: "year_label");

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "competitions",
                table: "Teams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "competitions",
                table: "Teams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Conference",
                schema: "competitions",
                table: "Teams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Division",
                schema: "competitions",
                table: "Teams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                schema: "competitions",
                table: "Teams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "BirthDate",
                schema: "competitions",
                table: "Players",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                schema: "competitions",
                table: "Players",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "HeightCm",
                schema: "competitions",
                table: "Players",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "WeightKg",
                schema: "competitions",
                table: "Players",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "JerseyNumber",
                schema: "competitions",
                table: "player_team_assignments",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                schema: "competitions",
                table: "Leagues",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortCode",
                schema: "competitions",
                table: "Leagues",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "competitions",
                table: "competitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "CurrentPeriod",
                schema: "competitions",
                table: "competitions",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                schema: "competitions",
                table: "competitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeRemaining",
                schema: "competitions",
                table: "competitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Venue",
                schema: "competitions",
                table: "competitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "minutes_played",
                schema: "competitions",
                table: "competition_rosters",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "position_played",
                schema: "competitions",
                table: "competition_rosters",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SportType",
                schema: "competitions",
                table: "Channels",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_ExternalId",
                schema: "competitions",
                table: "Teams",
                column: "ExternalId",
                unique: true,
                filter: "\"ExternalId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_LeagueId_year_label",
                schema: "competitions",
                table: "Seasons",
                columns: new[] { "LeagueId", "year_label" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_ExternalId",
                schema: "competitions",
                table: "Players",
                column: "ExternalId",
                unique: true,
                filter: "\"ExternalId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_ExternalId",
                schema: "competitions",
                table: "Leagues",
                column: "ExternalId",
                unique: true,
                filter: "\"ExternalId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_competitions_ExternalId",
                schema: "competitions",
                table: "competitions",
                column: "ExternalId",
                unique: true,
                filter: "\"ExternalId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_competitions_SeasonId_HomeTeamId_AwayTeamId_StartTime",
                schema: "competitions",
                table: "competitions",
                columns: new[] { "SeasonId", "HomeTeamId", "AwayTeamId", "StartTime" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teams_ExternalId",
                schema: "competitions",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Seasons_LeagueId_year_label",
                schema: "competitions",
                table: "Seasons");

            migrationBuilder.DropIndex(
                name: "IX_Players_ExternalId",
                schema: "competitions",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_Leagues_ExternalId",
                schema: "competitions",
                table: "Leagues");

            migrationBuilder.DropIndex(
                name: "IX_competitions_ExternalId",
                schema: "competitions",
                table: "competitions");

            migrationBuilder.DropIndex(
                name: "IX_competitions_SeasonId_HomeTeamId_AwayTeamId_StartTime",
                schema: "competitions",
                table: "competitions");

            migrationBuilder.DropColumn(
                name: "City",
                schema: "competitions",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "competitions",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "Conference",
                schema: "competitions",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "Division",
                schema: "competitions",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                schema: "competitions",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                schema: "competitions",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                schema: "competitions",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "HeightCm",
                schema: "competitions",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "WeightKg",
                schema: "competitions",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "JerseyNumber",
                schema: "competitions",
                table: "player_team_assignments");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                schema: "competitions",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "ShortCode",
                schema: "competitions",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "City",
                schema: "competitions",
                table: "competitions");

            migrationBuilder.DropColumn(
                name: "CurrentPeriod",
                schema: "competitions",
                table: "competitions");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                schema: "competitions",
                table: "competitions");

            migrationBuilder.DropColumn(
                name: "TimeRemaining",
                schema: "competitions",
                table: "competitions");

            migrationBuilder.DropColumn(
                name: "Venue",
                schema: "competitions",
                table: "competitions");

            migrationBuilder.DropColumn(
                name: "minutes_played",
                schema: "competitions",
                table: "competition_rosters");

            migrationBuilder.DropColumn(
                name: "position_played",
                schema: "competitions",
                table: "competition_rosters");

            migrationBuilder.DropColumn(
                name: "SportType",
                schema: "competitions",
                table: "Channels");

            migrationBuilder.RenameColumn(
                name: "Nickname",
                schema: "competitions",
                table: "Teams",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "year_label",
                schema: "competitions",
                table: "Seasons",
                newName: "Label");

            migrationBuilder.AddColumn<short>(
                name: "TeamSide",
                schema: "competitions",
                table: "competition_rosters",
                type: "smallint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_LeagueId",
                schema: "competitions",
                table: "Seasons",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_competitions_SeasonId",
                schema: "competitions",
                table: "competitions",
                column: "SeasonId");
        }
    }
}
