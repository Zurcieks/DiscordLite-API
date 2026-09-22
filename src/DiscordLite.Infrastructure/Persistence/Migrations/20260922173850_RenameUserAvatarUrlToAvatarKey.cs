using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscordLite.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserAvatarUrlToAvatarKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AvatarUrl",
                table: "Users",
                newName: "AvatarKey");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Conversations",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Conversations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Conversations");

            migrationBuilder.RenameColumn(
                name: "AvatarKey",
                table: "Users",
                newName: "AvatarUrl");
        }
    }
}
