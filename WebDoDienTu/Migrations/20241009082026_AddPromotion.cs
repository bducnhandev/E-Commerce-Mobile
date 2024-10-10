using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebDoDienTu.Migrations
{
    /// <inheritdoc />
    public partial class AddPromotion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Promotion_OrderPromotionId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPromotion_Products_ProductId",
                table: "ProductPromotion");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPromotion_Promotion_PromotionId",
                table: "ProductPromotion");

            migrationBuilder.DropForeignKey(
                name: "FK_WishList_AspNetUsers_UserId",
                table: "WishList");

            migrationBuilder.DropForeignKey(
                name: "FK_WishListItem_WishList_WishListId",
                table: "WishListItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WishList",
                table: "WishList");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Promotion",
                table: "Promotion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductPromotion",
                table: "ProductPromotion");

            migrationBuilder.RenameTable(
                name: "WishList",
                newName: "WishLists");

            migrationBuilder.RenameTable(
                name: "Promotion",
                newName: "Promotions");

            migrationBuilder.RenameTable(
                name: "ProductPromotion",
                newName: "ProductPromotions");

            migrationBuilder.RenameIndex(
                name: "IX_WishList_UserId",
                table: "WishLists",
                newName: "IX_WishLists_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductPromotion_PromotionId",
                table: "ProductPromotions",
                newName: "IX_ProductPromotions_PromotionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WishLists",
                table: "WishLists",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Promotions",
                table: "Promotions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductPromotions",
                table: "ProductPromotions",
                columns: new[] { "ProductId", "PromotionId" });

            migrationBuilder.UpdateData(
                table: "Promotions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2024, 10, 19, 15, 20, 25, 762, DateTimeKind.Local).AddTicks(6398), new DateTime(2024, 9, 29, 15, 20, 25, 762, DateTimeKind.Local).AddTicks(6381) });

            migrationBuilder.UpdateData(
                table: "Promotions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2024, 10, 24, 15, 20, 25, 762, DateTimeKind.Local).AddTicks(6408), new DateTime(2024, 10, 4, 15, 20, 25, 762, DateTimeKind.Local).AddTicks(6407) });

            migrationBuilder.UpdateData(
                table: "Promotions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2024, 10, 29, 15, 20, 25, 762, DateTimeKind.Local).AddTicks(6410), new DateTime(2024, 10, 8, 15, 20, 25, 762, DateTimeKind.Local).AddTicks(6410) });

            migrationBuilder.UpdateData(
                table: "Promotions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2024, 11, 8, 15, 20, 25, 762, DateTimeKind.Local).AddTicks(6413), new DateTime(2024, 9, 9, 15, 20, 25, 762, DateTimeKind.Local).AddTicks(6412) });

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Promotions_OrderPromotionId",
                table: "Orders",
                column: "OrderPromotionId",
                principalTable: "Promotions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPromotions_Products_ProductId",
                table: "ProductPromotions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPromotions_Promotions_PromotionId",
                table: "ProductPromotions",
                column: "PromotionId",
                principalTable: "Promotions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WishListItem_WishLists_WishListId",
                table: "WishListItem",
                column: "WishListId",
                principalTable: "WishLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WishLists_AspNetUsers_UserId",
                table: "WishLists",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Promotions_OrderPromotionId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPromotions_Products_ProductId",
                table: "ProductPromotions");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPromotions_Promotions_PromotionId",
                table: "ProductPromotions");

            migrationBuilder.DropForeignKey(
                name: "FK_WishListItem_WishLists_WishListId",
                table: "WishListItem");

            migrationBuilder.DropForeignKey(
                name: "FK_WishLists_AspNetUsers_UserId",
                table: "WishLists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WishLists",
                table: "WishLists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Promotions",
                table: "Promotions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductPromotions",
                table: "ProductPromotions");

            migrationBuilder.RenameTable(
                name: "WishLists",
                newName: "WishList");

            migrationBuilder.RenameTable(
                name: "Promotions",
                newName: "Promotion");

            migrationBuilder.RenameTable(
                name: "ProductPromotions",
                newName: "ProductPromotion");

            migrationBuilder.RenameIndex(
                name: "IX_WishLists_UserId",
                table: "WishList",
                newName: "IX_WishList_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductPromotions_PromotionId",
                table: "ProductPromotion",
                newName: "IX_ProductPromotion_PromotionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WishList",
                table: "WishList",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Promotion",
                table: "Promotion",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductPromotion",
                table: "ProductPromotion",
                columns: new[] { "ProductId", "PromotionId" });

            migrationBuilder.UpdateData(
                table: "Promotion",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2024, 10, 19, 15, 11, 12, 235, DateTimeKind.Local).AddTicks(2246), new DateTime(2024, 9, 29, 15, 11, 12, 235, DateTimeKind.Local).AddTicks(2228) });

            migrationBuilder.UpdateData(
                table: "Promotion",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2024, 10, 24, 15, 11, 12, 235, DateTimeKind.Local).AddTicks(2251), new DateTime(2024, 10, 4, 15, 11, 12, 235, DateTimeKind.Local).AddTicks(2250) });

            migrationBuilder.UpdateData(
                table: "Promotion",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2024, 10, 29, 15, 11, 12, 235, DateTimeKind.Local).AddTicks(2255), new DateTime(2024, 10, 8, 15, 11, 12, 235, DateTimeKind.Local).AddTicks(2254) });

            migrationBuilder.UpdateData(
                table: "Promotion",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2024, 11, 8, 15, 11, 12, 235, DateTimeKind.Local).AddTicks(2258), new DateTime(2024, 9, 9, 15, 11, 12, 235, DateTimeKind.Local).AddTicks(2257) });

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Promotion_OrderPromotionId",
                table: "Orders",
                column: "OrderPromotionId",
                principalTable: "Promotion",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPromotion_Products_ProductId",
                table: "ProductPromotion",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPromotion_Promotion_PromotionId",
                table: "ProductPromotion",
                column: "PromotionId",
                principalTable: "Promotion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WishList_AspNetUsers_UserId",
                table: "WishList",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WishListItem_WishList_WishListId",
                table: "WishListItem",
                column: "WishListId",
                principalTable: "WishList",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
