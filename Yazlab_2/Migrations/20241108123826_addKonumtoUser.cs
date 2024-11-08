using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yazlab_2.Migrations
{
    /// <inheritdoc />
    public partial class addKonumtoUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Konum",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Konum",
                table: "AspNetUsers");
        }
    }
}
