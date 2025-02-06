using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Poster", "Year" },
                values: new object[] { "https://example.com/posters/shawshank.jpg", "1994" });

            migrationBuilder.UpdateData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Poster", "Year" },
                values: new object[] { "https://example.com/posters/godfather.jpg", "1972" });

            migrationBuilder.UpdateData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Poster", "Year" },
                values: new object[] { "https://example.com/posters/darkknight.jpg", "2008" });

            migrationBuilder.UpdateData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Poster", "Year" },
                values: new object[] { "https://example.com/posters/forrestgump.jpg", "1994" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Poster", "Year" },
                values: new object[] { "", "" });

            migrationBuilder.UpdateData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Poster", "Year" },
                values: new object[] { "", "" });

            migrationBuilder.UpdateData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Poster", "Year" },
                values: new object[] { "", "" });

            migrationBuilder.UpdateData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Poster", "Year" },
                values: new object[] { "", "" });
        }
    }
}
