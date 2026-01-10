using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotorcycleShop.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserDiscounts_AspNetUsers_UserId",
                table: "UserDiscounts");

            migrationBuilder.DropForeignKey(
                name: "FK_UserDiscounts_Discounts_DiscountId",
                table: "UserDiscounts");

            migrationBuilder.UpdateData(
                table: "Discounts",
                keyColumn: "DiscountId",
                keyValue: 1,
                columns: new[] { "DateEnd", "DateStart" },
                values: new object[] { new DateTime(2026, 4, 10, 10, 30, 36, 172, DateTimeKind.Local).AddTicks(3908), new DateTime(2026, 1, 10, 10, 30, 36, 172, DateTimeKind.Local).AddTicks(3907) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 10, 10, 30, 36, 172, DateTimeKind.Local).AddTicks(3815));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 10, 10, 30, 36, 172, DateTimeKind.Local).AddTicks(3831));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 10, 10, 30, 36, 172, DateTimeKind.Local).AddTicks(3833));

            migrationBuilder.AddForeignKey(
                name: "FK_UserDiscounts_AspNetUsers_UserId",
                table: "UserDiscounts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserDiscounts_Discounts_DiscountId",
                table: "UserDiscounts",
                column: "DiscountId",
                principalTable: "Discounts",
                principalColumn: "DiscountId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserDiscounts_AspNetUsers_UserId",
                table: "UserDiscounts");

            migrationBuilder.DropForeignKey(
                name: "FK_UserDiscounts_Discounts_DiscountId",
                table: "UserDiscounts");

            migrationBuilder.UpdateData(
                table: "Discounts",
                keyColumn: "DiscountId",
                keyValue: 1,
                columns: new[] { "DateEnd", "DateStart" },
                values: new object[] { new DateTime(2026, 4, 9, 15, 42, 2, 76, DateTimeKind.Local).AddTicks(1326), new DateTime(2026, 1, 9, 15, 42, 2, 76, DateTimeKind.Local).AddTicks(1325) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 9, 15, 42, 2, 76, DateTimeKind.Local).AddTicks(1255));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 9, 15, 42, 2, 76, DateTimeKind.Local).AddTicks(1276));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 9, 15, 42, 2, 76, DateTimeKind.Local).AddTicks(1278));

            migrationBuilder.AddForeignKey(
                name: "FK_UserDiscounts_AspNetUsers_UserId",
                table: "UserDiscounts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserDiscounts_Discounts_DiscountId",
                table: "UserDiscounts",
                column: "DiscountId",
                principalTable: "Discounts",
                principalColumn: "DiscountId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
