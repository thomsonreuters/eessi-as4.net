using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Eu.EDelivery.AS4.Migrations
{
    public partial class AddRetryReliabilityTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RetryReliability",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                              .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RefToInMessageId = table.Column<long?>(nullable: true),
                    RefToOutMessageId = table.Column<long?>(nullable: true),
                    RefToInExceptionId = table.Column<long?>(nullable: true),
                    RefToOutExceptionId = table.Column<long?>(nullable: true),
                    RetryType = table.Column<string>(maxLength: 12),
                    CurrentRetryCount = table.Column<int>(nullable: false),
                    MaxRetryCount = table.Column<int>(nullable: false),
                    RetryInterval = table.Column<string>(maxLength: 50, nullable: false),
                    Status = table.Column<string>(maxLength: 25),
                    LastRetryTime = table.Column<DateTimeOffset>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetryReliability", x => x.Id);
                    
                });

            if (migrationBuilder.ActiveProvider.Equals(
                "Microsoft.EntityFrameworkCore.SqlServer",
                StringComparison.OrdinalIgnoreCase))
            {
                migrationBuilder.AddForeignKey(
                    table: "RetryReliability",
                    name: "FK_InMessages_Id_RetryReliability_RefToInMessageId",
                    column: "RefToInMessageId",
                    principalTable: "InMessages",
                    principalColumn: "Id");

                migrationBuilder.AddForeignKey(
                    table: "RetryReliability",
                    name: "FK_OutMessages_Id_RetryReliability_RefToOutMessageId",
                    column: "RefToOutMessageId",
                    principalTable: "OutMessages",
                    principalColumn: "Id");

                migrationBuilder.AddForeignKey(
                    table: "RetryReliability",
                    name: "FK_InExceptions_Id_RetryReliability_RefToInExceptionId",
                    column: "RefToInExceptionId",
                    principalTable: "InExceptions",
                    principalColumn: "Id");

                migrationBuilder.AddForeignKey(
                    table: "RetryReliability",
                    name: "FK_OutExceptions_Id_RetryReliability_RefToOutExceptionId",
                    column: "RefToOutExceptionId",
                    principalTable: "OutExceptions",
                    principalColumn: "Id");
            }

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("RetryReliability");
        }
    }
}
