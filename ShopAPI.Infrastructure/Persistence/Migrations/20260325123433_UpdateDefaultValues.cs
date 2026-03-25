using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ShopAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDefaultValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "ImageUrl", "IsActive", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { 1, "Electronics", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Premium noise-cancelling wireless headphones with 30hr battery", "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400", true, "Wireless Headphones", 79.99m, 50 },
                    { 2, "Electronics", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Feature-packed smartwatch with health tracking", "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400", true, "Smart Watch", 149.99m, 30 },
                    { 3, "Footwear", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lightweight performance running shoes", "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400", true, "Running Shoes", 89.99m, 75 },
                    { 4, "Kitchen", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "12-cup programmable coffee maker with thermal carafe", "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?w=400", true, "Coffee Maker", 49.99m, 40 },
                    { 5, "Accessories", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Slim genuine leather bifold wallet with RFID blocking", "https://images.unsplash.com/photo-1627123424574-724758594e93?w=400", true, "Leather Wallet", 29.99m, 100 },
                    { 6, "Bags", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Durable 30L travel backpack with laptop compartment", "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=400", true, "Backpack", 59.99m, 60 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "IsActive", "Name", "PasswordHash", "Role", "SecurityStamp" },
                values: new object[] { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@shop.com", true, "Admin", "", "admin", null });
        }
    }
}
