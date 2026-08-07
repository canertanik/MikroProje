using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MikroProje.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedDate",
                table: "Purchases",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Purchases",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // Mevcut kayıtların stokları zaten artırılmış durumda, onları Received (2) olarak işaretle
            migrationBuilder.Sql("UPDATE Purchases SET Status = 2, ReceivedDate = CreatedDate WHERE IsDeleted = 0");
            migrationBuilder.Sql("UPDATE Purchases SET Status = 3 WHERE IsDeleted = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceivedDate",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Purchases");
        }
    }
}
