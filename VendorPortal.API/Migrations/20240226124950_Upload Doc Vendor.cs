using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendorPortal.API.Migrations
{
    /// <inheritdoc />
    public partial class UploadDocVendor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentPaths",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentPaths",
                table: "AspNetUsers");
        }
    }
}
