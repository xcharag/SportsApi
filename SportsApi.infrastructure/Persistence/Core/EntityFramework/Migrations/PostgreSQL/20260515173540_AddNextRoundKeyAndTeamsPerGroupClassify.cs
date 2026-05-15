using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsApi.infrastructure.Persistence.Core.EntityFramework.Migrations.PostgreSQL
{
    /// <inheritdoc />
    public partial class AddNextRoundKeyAndTeamsPerGroupClassify : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeamsPerGroupThatClassify",
                table: "Tournaments",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<string>(
                name: "NextRoundKey",
                table: "RoundsClassified",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeamsPerGroupThatClassify",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "NextRoundKey",
                table: "RoundsClassified");
        }
    }
}
