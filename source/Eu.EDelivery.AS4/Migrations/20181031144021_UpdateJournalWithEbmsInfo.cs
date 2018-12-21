using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Eu.EDelivery.AS4.Migrations
{
    public partial class UpdateJournalWithEbmsInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider.Equals("Microsoft.EntityFrameworkCore.Sqlite",
                                                       StringComparison.OrdinalIgnoreCase))
            {
                migrationBuilder.RenameTable("Journal", newName: "OldJournal");

                migrationBuilder.CreateTable(
                    name: "Journal",
                    columns: table => new
                    {
                        Id = table.Column<long>(nullable: false)
                                  .Annotation("SqlServer:ValueGenerationStrategy",
                                              SqlServerValueGenerationStrategy.IdentityColumn),
                        RefToInMessageId = table.Column<long?>(nullable: true),
                        RefToOutMessageId = table.Column<long?>(nullable: true),
                        LogEntry = table.Column<string>(nullable: false),
                        LogDate = table.Column<DateTimeOffset>(nullable: false),
                        AgentType = table.Column<string>(nullable: false, maxLength: 20),
                        AgentName = table.Column<string>(nullable: false, maxLength: 50),
                        EbmsMessageId = table.Column<string>(nullable: true, maxLength: 100),
                        RefToEbmsMessageId = table.Column<string>(nullable: true, maxLength: 100),
                        FromParty = table.Column<string>(nullable: true, maxLength: 255),
                        ToParty = table.Column<string>(nullable: true, maxLength: 255),
                        Service = table.Column<string>(nullable: true, maxLength: 255),
                        Action = table.Column<string>(nullable: true, maxLength: 255)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Journal", x => x.Id);
                    });

                migrationBuilder.Sql(
                    "INSERT INTO Journal (RefToInMessageId, RefToOutMessageId, LogEntry, LogDate, AgentType, AgentName, EbmsMessageId) "
                    + "SELECT RefToInMessageId, RefToOutMessageId, LogEntry, LogDate, AgentType, AgentName, EbmsMessageId "
                    + "FROM OldJournal");

                migrationBuilder.DropTable("OldJournal");

                migrationBuilder.CreateIndex(
                    name: "IX_Journal_RefToInMessageId",
                    table: "Journal",
                    column: "RefToInMessageId");

                migrationBuilder.CreateIndex(
                    name: "IX_Journal_RefToOutMessageId",
                    table: "Journal",
                    column: "RefToOutMessageId");
            }
            else
            {
                migrationBuilder.AddColumn<string>(
                    name: "Action",
                    table: "Journal",
                    maxLength: 255,
                    nullable: true);

                migrationBuilder.AddColumn<string>(
                    name: "FromParty",
                    table: "Journal",
                    maxLength: 255,
                    nullable: true);

                migrationBuilder.AddColumn<string>(
                    name: "RefToEbmsMessageId",
                    table: "Journal",
                    maxLength: 100,
                    nullable: true);

                migrationBuilder.AddColumn<string>(
                    name: "Service",
                    table: "Journal",
                    maxLength: 255,
                    nullable: true);

                migrationBuilder.AddColumn<string>(
                    name: "ToParty",
                    table: "Journal",
                    maxLength: 255,
                    nullable: true);

                migrationBuilder.DropColumn(
                    name: "MessageStatus",
                    table: "Journal");

                migrationBuilder.DropColumn(
                    name: "MessageOperation",
                    table: "Journal");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider.Equals("Microsoft.EntityFrameworkCore.Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                migrationBuilder.RenameTable("Journal", newName: "OldJournal");

                migrationBuilder.CreateTable(
                    name: "Journal",
                    columns: table => new
                    {
                        Id = table.Column<long>(nullable: false)
                                  .Annotation("SqlServer:ValueGenerationStrategy",
                                              SqlServerValueGenerationStrategy.IdentityColumn),
                        RefToInMessageId = table.Column<long?>(nullable: true),
                        RefToOutMessageId = table.Column<long?>(nullable: true),
                        LogEntry = table.Column<string>(nullable: false),
                        LogDate = table.Column<DateTimeOffset>(nullable: false),
                        AgentType = table.Column<string>(nullable: false, maxLength: 20),
                        AgentName = table.Column<string>(nullable: false, maxLength: 50),
                        EbmsMessageId = table.Column<string>(nullable: false, maxLength: 100),
                        MessageStatus = table.Column<string>(nullable: false, maxLength: 20),
                        MessageOperation = table.Column<string>(nullable: false, maxLength: 20)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Journal", x => x.Id);
                    });

                migrationBuilder.Sql(
                    "INSERT INTO Journal (RefToInMessageId, RefToOutMessageId, LogEntry, LogDate, AgentType, AgentName, EbmsMessageId, MessageStatus, MessageOperation) "
                    + "SELECT RefToInMessageId, RefToOutMessageId, LogEntry, LogDate, AgentType, AgentName, EbmsMessageId, MessageStatus, MessageOperation "
                    + "FROM OldJournal",
                    suppressTransaction: true);

                migrationBuilder.DropTable("OldJournal");

                migrationBuilder.CreateIndex(
                    name: "IX_Journal_RefToInMessageId",
                    table: "Journal",
                    column: "RefToInMessageId",
                    unique: false);

                migrationBuilder.CreateIndex(
                    name: "IX_Journal_RefToOutMessageId",
                    table: "Journal",
                    column: "RefToOutMessageId",
                    unique: false);
            }
            else
            {
                migrationBuilder.DropColumn(
                    name: "Action",
                    table: "Journal");

                migrationBuilder.DropColumn(
                    name: "FromParty",
                    table: "Journal");

                migrationBuilder.DropColumn(
                    name: "RefToEbmsMessageId",
                    table: "Journal");

                migrationBuilder.DropColumn(
                    name: "Service",
                    table: "Journal");

                migrationBuilder.DropColumn(
                    name: "ToParty",
                    table: "Journal");
            }
        }
    }
}
