using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApeSats.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateBidAccessibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Arts_Bids_BidId",
                table: "Arts");

            migrationBuilder.AlterColumn<int>(
                name: "BidId",
                table: "Arts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Arts_Bids_BidId",
                table: "Arts",
                column: "BidId",
                principalTable: "Bids",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Arts_Bids_BidId",
                table: "Arts");

            migrationBuilder.AlterColumn<int>(
                name: "BidId",
                table: "Arts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Arts_Bids_BidId",
                table: "Arts",
                column: "BidId",
                principalTable: "Bids",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
