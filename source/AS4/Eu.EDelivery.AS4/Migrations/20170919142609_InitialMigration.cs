using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Eu.EDelivery.AS4.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InExceptions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EbmsRefToMessageId = table.Column<string>(nullable: true),
                    Exception = table.Column<string>(nullable: true),
                    InsertionTime = table.Column<DateTimeOffset>(nullable: false),
                    MessageBody = table.Column<byte[]>(nullable: true),
                    ModificationTime = table.Column<DateTimeOffset>(nullable: false),
                    Operation = table.Column<string>(maxLength: 50, nullable: true),
                    PMode = table.Column<string>(nullable: true),
                    PModeId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InExceptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InMessages",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                        
                    Action = table.Column<string>(maxLength: 255, nullable: true),
                    ContentType = table.Column<string>(maxLength: 256, nullable: true),
                    ConversationId = table.Column<string>(maxLength: 50, nullable: true),
                    EbmsMessageId = table.Column<string>(nullable: true),
                    EbmsMessageType = table.Column<string>(nullable: true),
                    EbmsRefToMessageId = table.Column<string>(nullable: true),
                    FromParty = table.Column<string>(maxLength: 255, nullable: true),
                    InsertionTime = table.Column<DateTimeOffset>(nullable: false),
                    Intermediary = table.Column<bool>(nullable: false),
                    IsDuplicate = table.Column<bool>(nullable: false),
                    IsTest = table.Column<bool>(nullable: false),
                    MEP = table.Column<string>(maxLength: 25, nullable: true),
                    MessageLocation = table.Column<string>(maxLength: 512, nullable: true),
                    ModificationTime = table.Column<DateTimeOffset>(nullable: false),
                    MPC = table.Column<string>(maxLength: 255, nullable: true),
                    Operation = table.Column<string>(maxLength: 50, nullable: true),
                    PMode = table.Column<string>(nullable: true),
                    PModeId = table.Column<string>(nullable: true),
                    Service = table.Column<string>(maxLength: 255, nullable: true),
                    SoapEnvelope = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    ToParty = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutExceptions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EbmsRefToMessageId = table.Column<string>(nullable: true),
                    Exception = table.Column<string>(nullable: true),
                    InsertionTime = table.Column<DateTimeOffset>(nullable: false),
                    MessageBody = table.Column<byte[]>(nullable: true),
                    ModificationTime = table.Column<DateTimeOffset>(nullable: false),
                    Operation = table.Column<string>(maxLength: 50, nullable: true),
                    PMode = table.Column<string>(nullable: true),
                    PModeId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutExceptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutMessages",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Action = table.Column<string>(maxLength: 255, nullable: true),
                    ContentType = table.Column<string>(maxLength: 256, nullable: true),
                    ConversationId = table.Column<string>(maxLength: 50, nullable: true),
                    EbmsMessageId = table.Column<string>(nullable: false),
                    EbmsMessageType = table.Column<string>(nullable: true),
                    EbmsRefToMessageId = table.Column<string>(nullable: true),
                    FromParty = table.Column<string>(maxLength: 255, nullable: true),
                    InsertionTime = table.Column<DateTimeOffset>(nullable: false),
                    Intermediary = table.Column<bool>(nullable: false),
                    IsDuplicate = table.Column<bool>(nullable: false),
                    IsTest = table.Column<bool>(nullable: false),
                    MEP = table.Column<string>(maxLength: 25, nullable: true),
                    MessageLocation = table.Column<string>(maxLength: 512, nullable: true),
                    ModificationTime = table.Column<DateTimeOffset>(nullable: false),
                    MPC = table.Column<string>(maxLength: 255, nullable: true),
                    Operation = table.Column<string>(maxLength: 50, nullable: true),
                    PMode = table.Column<string>(nullable: true),
                    PModeId = table.Column<string>(nullable: true),
                    Service = table.Column<string>(maxLength: 255, nullable: true),
                    SoapEnvelope = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    ToParty = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutMessages", x => x.Id);
                    table.UniqueConstraint("AK_OutMessages_EbmsMessageId", x => x.EbmsMessageId);
                });

            migrationBuilder.CreateTable(
                name: "ReceptionAwareness",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CurrentRetryCount = table.Column<int>(nullable: false),
                    InsertionTime = table.Column<DateTimeOffset>(nullable: false),
                    InternalMessageId = table.Column<string>(nullable: false),
                    LastSendTime = table.Column<DateTimeOffset>(nullable: true),
                    ModificationTime = table.Column<DateTimeOffset>(nullable: false),
                    RetryInterval = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    TotalRetryCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceptionAwareness", x => x.Id);
                    table.UniqueConstraint("AK_ReceptionAwareness_InternalMessageId", x => x.InternalMessageId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InExceptions_EbmsRefToMessageId",
                table: "InExceptions",
                column: "EbmsRefToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_InExceptions_Operation",
                table: "InExceptions",
                column: "Operation");

            migrationBuilder.CreateIndex(
                name: "IX_InMessages_EbmsRefToMessageId",
                table: "InMessages",
                column: "EbmsRefToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_InMessages_EbmsMessageId_IsDuplicate",
                table: "InMessages",
                columns: new[] { "EbmsMessageId", "IsDuplicate" });

            migrationBuilder.CreateIndex(
                name: "IX_InMessages_Operation_InsertionTime",
                table: "InMessages",
                columns: new[] { "Operation", "InsertionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_OutExceptions_EbmsRefToMessageId",
                table: "OutExceptions",
                column: "EbmsRefToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_OutExceptions_Operation",
                table: "OutExceptions",
                column: "Operation");

            migrationBuilder.CreateIndex(
                name: "IX_OutMessages_EbmsRefToMessageId",
                table: "OutMessages",
                column: "EbmsRefToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_OutMessages_InsertionTime",
                table: "OutMessages",
                column: "InsertionTime");

            migrationBuilder.CreateIndex(
                name: "IX_OutMessages_Operation_MEP_MPC_InsertionTime",
                table: "OutMessages",
                columns: new[] { "Operation", "MEP", "MPC", "InsertionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ReceptionAwareness_Status_CurrentRetryCount",
                table: "ReceptionAwareness",
                columns: new[] { "Status", "CurrentRetryCount" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InExceptions");

            migrationBuilder.DropTable(
                name: "InMessages");

            migrationBuilder.DropTable(
                name: "OutExceptions");

            migrationBuilder.DropTable(
                name: "OutMessages");

            migrationBuilder.DropTable(
                name: "ReceptionAwareness");
        }
    }
}
