using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Eu.EDelivery.AS4.Migrations
{
    public partial class AddJournalTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Journal",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                              .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RefToInMessageId = table.Column<long?>(nullable: true),
                    RefToOutMessageId = table.Column<long?>(nullable: true),
                    LogEntry = table.Column<string>(nullable: false, maxLength: 100),
                    LogDate = table.Column<DateTimeOffset>(nullable: false),
                    AgentType = table.Column<string>(nullable: false, maxLength: 20),
                    AgentName = table.Column<string>(nullable: false, maxLength: 50),
                    EbmsMessageId = table.Column<string>(nullable: false, maxLength: 100),
                    MessageStatus = table.Column<string>(nullable: false, maxLength: 20),
                    MessageOperation = table.Column<string>(nullable: false, maxLength: 20)
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
                    table: "Journal",
                    name: "FK_InMessages_Id_Journal_RefToInMessageId",
                    column: "RefToInMessageId",
                    principalTable: "InMessages",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);

                migrationBuilder.AddForeignKey(
                    table: "Journal",
                    name: "FK_OutMessages_Id_Journal_RefToOutMessageId",
                    column: "RefToOutMessageId",
                    principalTable: "OutMessages",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            }

            migrationBuilder.CreateIndex(
                name: "IX_Journal_RefToInMessageId",
                table: "RetryReliability",
                column: "RefToInMessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Journal_RefToOutMessageId",
                table: "RetryReliability",
                column: "RefToOutMessageId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Journal");
        }
    }
}
