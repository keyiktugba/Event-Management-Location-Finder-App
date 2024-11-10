using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yazlab_2.Migrations
{
    /// <inheritdoc />
    public partial class AddIsApprovedToEtkinlik : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Etkinlikler",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Etkinlikler");
        }
    }
}
