using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApeSats.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class artNumer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Arts_Bids_BidId",
                table: "Arts");

            migrationBuilder.DropIndex(
                name: "IX_Arts_BidId",
                table: "Arts");

            migrationBuilder.DropColumn(
                name: "BidId",
                table: "Arts");

            migrationBuilder.AddColumn<int>(
                name: "ArtNumber",
                table: "Bids",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "Bids",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "Bids",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArtNumber",
                table: "Bids");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "Bids");

            migrationBuilder.DropColumn(
                name: "Reference",
                table: "Bids");

            migrationBuilder.AddColumn<int>(
                name: "BidId",
                table: "Arts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Arts_BidId",
                table: "Arts",
                column: "BidId");

            migrationBuilder.AddForeignKey(
                name: "FK_Arts_Bids_BidId",
                table: "Arts",
                column: "BidId",
                principalTable: "Bids",
                principalColumn: "Id");
        }
    }
}
