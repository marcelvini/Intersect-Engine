using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Intersect.Server.Migrations.Game
{
    public partial class ChainSpells : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Combat_ChainSpellDelay",
                table: "Spells",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "Combat_ChainSpellId",
                table: "Spells",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Combat_ChainSpellDelay",
                table: "Spells");

            migrationBuilder.DropColumn(
                name: "Combat_ChainSpellId",
                table: "Spells");
        }
    }
}
