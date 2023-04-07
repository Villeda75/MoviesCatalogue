using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MoviesCatalogue.Migrations
{
    /// <inheritdoc />
    public partial class Seed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Category", "CreatedDate", "ReleaseYear" },
                values: new object[] { "Fantasy", new DateTime(2023, 4, 7, 23, 46, 52, 543, DateTimeKind.Utc).AddTicks(957), 2001 });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Category", "CreatedDate", "ReleaseYear", "UserId" },
                values: new object[] { "Fantasy", new DateTime(2023, 4, 7, 23, 46, 52, 543, DateTimeKind.Utc).AddTicks(963), 2002, 1 });

            migrationBuilder.InsertData(
                table: "Movies",
                columns: new[] { "Id", "Category", "CreatedDate", "ImagePoster", "Name", "ReleaseYear", "Synopsis", "UserId" },
                values: new object[,]
                {
                    { 3, "Sports drama", new DateTime(2023, 4, 7, 23, 46, 52, 543, DateTimeKind.Utc).AddTicks(965), null, "Rocky", 1976, "Rocky is a small-time Philadelphia boxer going nowhere, until an unbelievable shot to fight the world heavyweight champion lights a fire inside him.", 2 },
                    { 4, "Kids", new DateTime(2023, 4, 7, 23, 46, 52, 543, DateTimeKind.Utc).AddTicks(967), null, "Bee Movie", 2007, "Barry, a worker bee stuck in a dead-end job making honey, sues humans when he learns they've been stealing bees' nectar all along.", 2 },
                    { 5, "Action & Adventure", new DateTime(2023, 4, 7, 23, 46, 52, 543, DateTimeKind.Utc).AddTicks(969), null, "Fast Five", 2011, "Brian and Mia break Dom out of prison and head to Brazil to put together a racing team -- and take on a drug dealer who wants to kill them.", 1 }
                });

            migrationBuilder.UpdateData(
                table: "RatedMovies",
                keyColumn: "Id",
                keyValue: 1,
                column: "Rate",
                value: 9);

            migrationBuilder.UpdateData(
                table: "RatedMovies",
                keyColumn: "Id",
                keyValue: 2,
                column: "Rate",
                value: 7);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Category", "CreatedDate", "ReleaseYear" },
                values: new object[] { "Horror", new DateTime(1965, 7, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 1998 });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Category", "CreatedDate", "ReleaseYear", "UserId" },
                values: new object[] { "Science fiction", new DateTime(2023, 3, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 2020, 2 });

            migrationBuilder.UpdateData(
                table: "RatedMovies",
                keyColumn: "Id",
                keyValue: 1,
                column: "Rate",
                value: 7);

            migrationBuilder.UpdateData(
                table: "RatedMovies",
                keyColumn: "Id",
                keyValue: 2,
                column: "Rate",
                value: 9);
        }
    }
}
