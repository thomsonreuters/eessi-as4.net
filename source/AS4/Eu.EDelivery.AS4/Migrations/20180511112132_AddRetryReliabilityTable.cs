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
                    CurrentRetryCount = table.Column<int>(defaultValue: 0),
                    MaxRetryCount = table.Column<int>(defaultValue: 0),
                    RetryInterval = table.Column<string>(maxLength: 50, defaultValue: "0:00:00:00,0000000"),
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
                    name: "FK_OutExceptions_Id_RetryReliability_RefToOutExceptioneId",
                    column: "RefToOutExceptionId",
                    principalTable: "OutExceptions",
                    principalColumn: "Id");
            }

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("RetryReliability");
        }
    }
}
