using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MotorcycleShop.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminAndPromotionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Coupon_AspNetUsers_UserId",
                table: "Coupon");

            migrationBuilder.DropForeignKey(
                name: "FK_Coupon_Discounts_DiscountId",
                table: "Coupon");

            migrationBuilder.DropForeignKey(
                name: "FK_Promotion_Discounts_DiscountId",
                table: "Promotion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Promotion",
                table: "Promotion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Coupon",
                table: "Coupon");

            migrationBuilder.RenameTable(
                name: "Promotion",
                newName: "Promotions");

            migrationBuilder.RenameTable(
                name: "Coupon",
                newName: "Coupons");

            migrationBuilder.RenameIndex(
                name: "IX_Promotion_DiscountId",
                table: "Promotions",
                newName: "IX_Promotions_DiscountId");

            migrationBuilder.RenameIndex(
                name: "IX_Coupon_UserId",
                table: "Coupons",
                newName: "IX_Coupons_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Coupon_DiscountId",
                table: "Coupons",
                newName: "IX_Coupons_DiscountId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Promotions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Promotions",
                table: "Promotions",
                column: "PromotionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Coupons",
                table: "Coupons",
                column: "CouponId");

            migrationBuilder.CreateTable(
                name: "AdminLogs",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Entity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminLogs", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_AdminLogs_AspNetUsers_AdminId",
                        column: x => x.AdminId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Discounts",
                keyColumn: "DiscountId",
                keyValue: 1,
                columns: new[] { "DateEnd", "DateStart" },
                values: new object[] { new DateTime(2026, 4, 12, 15, 54, 42, 305, DateTimeKind.Local).AddTicks(6721), new DateTime(2026, 1, 12, 15, 54, 42, 305, DateTimeKind.Local).AddTicks(6720) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 12, 15, 54, 42, 305, DateTimeKind.Local).AddTicks(6648));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 12, 15, 54, 42, 305, DateTimeKind.Local).AddTicks(6663));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 12, 15, 54, 42, 305, DateTimeKind.Local).AddTicks(6665));

            migrationBuilder.InsertData(
                table: "Promotions",
                columns: new[] { "PromotionId", "Condition", "Description", "DiscountId", "DisplayOrder", "EndDate", "ImagePath", "IsActive", "StartDate" },
                values: new object[,]
                {
                    { 1, "First Order Discount", "Get 20% off on your first purchase", 1, 1, new DateTime(2026, 7, 12, 15, 54, 42, 305, DateTimeKind.Local).AddTicks(6744), "/images/promotions/first-order.jpg", true, new DateTime(2026, 1, 12, 15, 54, 42, 305, DateTimeKind.Local).AddTicks(6744) },
                    { 2, "Free Shipping Over 2M", "Free shipping for orders over 2,000,000 VND", null, 2, new DateTime(2026, 4, 12, 15, 54, 42, 305, DateTimeKind.Local).AddTicks(6747), "/images/promotions/free-shipping.jpg", true, new DateTime(2026, 1, 12, 15, 54, 42, 305, DateTimeKind.Local).AddTicks(6747) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminLogs_AdminId",
                table: "AdminLogs",
                column: "AdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Coupons_AspNetUsers_UserId",
                table: "Coupons",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Coupons_Discounts_DiscountId",
                table: "Coupons",
                column: "DiscountId",
                principalTable: "Discounts",
                principalColumn: "DiscountId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Promotions_Discounts_DiscountId",
                table: "Promotions",
                column: "DiscountId",
                principalTable: "Discounts",
                principalColumn: "DiscountId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Coupons_AspNetUsers_UserId",
                table: "Coupons");

            migrationBuilder.DropForeignKey(
                name: "FK_Coupons_Discounts_DiscountId",
                table: "Coupons");

            migrationBuilder.DropForeignKey(
                name: "FK_Promotions_Discounts_DiscountId",
                table: "Promotions");

            migrationBuilder.DropTable(
                name: "AdminLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Promotions",
                table: "Promotions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Coupons",
                table: "Coupons");

            migrationBuilder.DeleteData(
                table: "Promotions",
                keyColumn: "PromotionId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Promotions",
                keyColumn: "PromotionId",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Promotions");

            migrationBuilder.RenameTable(
                name: "Promotions",
                newName: "Promotion");

            migrationBuilder.RenameTable(
                name: "Coupons",
                newName: "Coupon");

            migrationBuilder.RenameIndex(
                name: "IX_Promotions_DiscountId",
                table: "Promotion",
                newName: "IX_Promotion_DiscountId");

            migrationBuilder.RenameIndex(
                name: "IX_Coupons_UserId",
                table: "Coupon",
                newName: "IX_Coupon_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Coupons_DiscountId",
                table: "Coupon",
                newName: "IX_Coupon_DiscountId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Promotion",
                table: "Promotion",
                column: "PromotionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Coupon",
                table: "Coupon",
                column: "CouponId");

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
                name: "FK_Coupon_AspNetUsers_UserId",
                table: "Coupon",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Coupon_Discounts_DiscountId",
                table: "Coupon",
                column: "DiscountId",
                principalTable: "Discounts",
                principalColumn: "DiscountId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Promotion_Discounts_DiscountId",
                table: "Promotion",
                column: "DiscountId",
                principalTable: "Discounts",
                principalColumn: "DiscountId");
        }
    }
}
