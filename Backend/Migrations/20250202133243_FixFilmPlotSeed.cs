using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class FixFilmPlotSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Films");

            migrationBuilder.InsertData(
                table: "Films",
                columns: new[] { "Id", "Genre", "Imdb", "ImdbRating", "Name", "Plot", "Poster", "Year" },
                values: new object[,]
                {
                    { 1, "Drama", "tt0111161", "9.3", "The Shawshank Redemption", "Two imprisoned men bond over a number of years.", "", "" },
                    { 2, "Crime", "tt0068646", "9.2", "The Godfather", "The aging patriarch of an organized crime dynasty transfers control to his reluctant son.", "", "" },
                    { 3, "Action", "tt0468569", "9.0", "The Dark Knight", "When the menace known as the Joker emerges, he plunges Gotham into chaos.", "", "" },
                    { 4, "Drama", "tt0109830", "8.8", "Forrest Gump", "The presidencies of Kennedy and Johnson, the events of Vietnam, Watergate, and other history unfold through the perspective of an Alabama man.", "", "" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Password", "Pseudo", "Role" },
                values: new object[,]
                {
                    { 1, "defaultPassword", "Alice", 0 },
                    { 2, "defaultPassword", "Bob", 0 }
                });

            migrationBuilder.InsertData(
                table: "Favourites",
                columns: new[] { "Id", "FilmId", "Rating", "UserId" },
                values: new object[,]
                {
                    { 1, 1, 10, 1 },
                    { 2, 2, 9, 1 },
                    { 3, 3, 8, 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Favourites",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Favourites",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Favourites",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Films",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.AddColumn<float>(
                name: "Rating",
                table: "Films",
                type: "REAL",
                nullable: true);
        }
    }
}
