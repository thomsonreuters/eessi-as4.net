using Microsoft.EntityFrameworkCore.Migrations;

namespace Eu.EDelivery.AS4.Migrations
{
    public partial class UnsetUniqueRetryReliabilityIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RetryReliability_RefToInMessageId",
                table: "RetryReliability");

            migrationBuilder.DropIndex(
                name: "IX_RetryReliability_RefToOutMessageId",
                table: "RetryReliability");

            migrationBuilder.DropIndex(
                name: "IX_RetryReliability_RefToInExceptionId",
                table: "RetryReliability");

            migrationBuilder.DropIndex(
                name: "IX_RetryReliability_RefToOutExceptionId",
                table: "RetryReliability");

            migrationBuilder.CreateIndex(
                name: "IX_RetryReliability_RefToInMessageId",
                table: "RetryReliability",
                column: "RefToInMessageId",
                unique: false);

            migrationBuilder.CreateIndex(
                name: "IX_RetryReliability_RefToOutMessageId",
                table: "RetryReliability",
                column: "RefToOutMessageId",
                unique: false);

            migrationBuilder.CreateIndex(
                name: "IX_RetryReliability_RefToInExceptionId",
                table: "RetryReliability",
                column: "RefToInExceptionId",
                unique: false);

            migrationBuilder.CreateIndex(
                name: "IX_RetryReliability_RefToOutExceptionId",
                table: "RetryReliability",
                column: "RefToOutExceptionId",
                unique: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RetryReliability_RefToInMessageId",
                table: "RetryReliability");

            migrationBuilder.DropIndex(
                name: "IX_RetryReliability_RefToOutMessageId",
                table: "RetryReliability");

            migrationBuilder.DropIndex(
                name: "IX_RetryReliability_RefToInExceptionId",
                table: "RetryReliability");

            migrationBuilder.DropIndex(
                name: "IX_RetryReliability_RefToOutExceptionId",
                table: "RetryReliability");

            migrationBuilder.CreateIndex(
                name: "IX_RetryReliability_RefToInMessageId",
                table: "RetryReliability",
                column: "RefToInMessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RetryReliability_RefToOutMessageId",
                table: "RetryReliability",
                column: "RefToOutMessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RetryReliability_RefToInExceptionId",
                table: "RetryReliability",
                column: "RefToInExceptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RetryReliability_RefToOutExceptionId",
                table: "RetryReliability",
                column: "RefToOutExceptionId",
                unique: true);
        }
    }
}
