using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yazlab_2.Migrations
{
    /// <inheritdoc />
    public partial class AddFotoToEtkinlik : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Foto",
                table: "Etkinlikler",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Foto",
                table: "Etkinlikler");
        }
    }
}
