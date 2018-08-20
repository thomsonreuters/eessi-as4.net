using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Eu.EDelivery.AS4.Migrations
{
    public partial class RemoveOutMessagesConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider.Equals("Microsoft.EntityFrameworkCore.Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                DropAndRecreateOutMessageTable(migrationBuilder);
                DropAndRecreateReceptionAwarenessTable(migrationBuilder);
            }
            else
            {
                AlterOutMessagesTable(migrationBuilder);
                AlterReceptionAwarenessTable(migrationBuilder);
                CreateForeignKeyRelationShip(migrationBuilder);
            }
        }

        private static void AlterOutMessagesTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_OutMessages_EbmsMessageId",
                table: "OutMessages");

            migrationBuilder.CreateIndex(
                name: "IX_OutMessages_EbmsMessageId",
                table: "OutMessages",
                column: "EbmsMessageId");
        }

        private static void AlterReceptionAwarenessTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_ReceptionAwareness_InternalMessageId",
                table: "ReceptionAwareness");

            migrationBuilder.RenameColumn("InternalMessageId", "ReceptionAwareness", "RefToEbmsMessageId");

            migrationBuilder.AddColumn<long>(
                name: "RefToOutMessageId",
                table: "ReceptionAwareness",
                nullable: true,
                defaultValue: 0L);

            migrationBuilder.Sql("UPDATE ReceptionAwareness " +
                                 "   SET RefToOutMessageId = o.Id " +
                                 "  FROM OutMessages o " +
                                 " WHERE o.EbmsMessageId = ReceptionAwareness.RefToEbmsMessageId");

            migrationBuilder.AlterColumn<string>(
                name: "RefToOutMessageId",
                table: "ReceptionAwareness",
                nullable: false);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ReceptionAwareness_RefToOutMessageId",
                table: "ReceptionAwareness",
                column: "RefToOutMessageId");
        }

        private static void CreateForeignKeyRelationShip(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                table: "ReceptionAwareness",
                name: "FK_OutMessages_Id_ReceptionAwareness_RefToOutMessageId",
                column: "RefToOutMessageId",
                principalColumn: "Id",
                principalTable: "OutMessages",
                onDelete: ReferentialAction.Cascade);
        }

        private static void DropAndRecreateOutMessageTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("OutMessages", newName: "OldOutMessages");

            migrationBuilder.CreateTable(
                name: "OutMessages",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                              .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Action = table.Column<string>(maxLength: 255, nullable: true),
                    ContentType = table.Column<string>(maxLength: 256, nullable: true),
                    ConversationId = table.Column<string>(maxLength: 50, nullable: true),
                    EbmsMessageId = table.Column<string>(maxLength: 256, nullable: false),
                    EbmsMessageType = table.Column<string>(maxLength: 50, nullable: true),
                    EbmsRefToMessageId = table.Column<string>(maxLength: 256, nullable: true),
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
                    PModeId = table.Column<string>(maxLength: 256, nullable: true),
                    Service = table.Column<string>(maxLength: 255, nullable: true),
                    SoapEnvelope = table.Column<string>(nullable: true),
                    Status = table.Column<string>(maxLength: 50, nullable: true),
                    ToParty = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutMessages", x => x.Id);
                });

            migrationBuilder.Sql("INSERT INTO OutMessages " +
                                 "(Action, ContentType, ConversationId, EbmsMessageId, EbmsMessageType, " +
                                 " EbmsRefToMessageId, FromParty, InsertionTime, Intermediary, IsDuplicate, " +
                                 " IsTest, MEP, MessageLocation, ModificationTime, MPC, Operation, PMode, " +
                                 " PModeId, Service, SoapEnvelope, Status, ToParty) " +
                                 "SELECT Action, ContentType, ConversationId, EbmsMessageId, EbmsMessageType, " +
                                 " EbmsRefToMessageId, FromParty, InsertionTime, Intermediary, IsDuplicate, " +
                                 " IsTest, MEP, MessageLocation, ModificationTime, MPC, Operation, PMode, " +
                                 " PModeId, Service, SoapEnvelope, Status, ToParty " +
                                 "FROM OldOutMessages", suppressTransaction: true);

            migrationBuilder.DropTable("OldOutMessages");

            migrationBuilder.CreateIndex(
                    name: "IX_OutMessages_EbmsMessageId",
                    table: "OutMessages",
                    column: "EbmsMessageId");

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
        }

        private static void DropAndRecreateReceptionAwarenessTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("ReceptionAwareness", newName: "OldReceptionAwareness");

            migrationBuilder.CreateTable(
                name: "ReceptionAwareness",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                              .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CurrentRetryCount = table.Column<int>(nullable: false),
                    InsertionTime = table.Column<DateTimeOffset>(nullable: false),
                    RefToEbmsMessageId = table.Column<string>(maxLength: 256, nullable: false),
                    RefToOutMessageId = table.Column<long>(nullable:false),
                    LastSendTime = table.Column<DateTimeOffset>(nullable: true),
                    ModificationTime = table.Column<DateTimeOffset>(nullable: false),
                    RetryInterval = table.Column<string>(maxLength: 12, nullable: true),
                    Status = table.Column<string>(maxLength: 25, nullable: true),
                    TotalRetryCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceptionAwareness", x => x.Id);
                    table.UniqueConstraint("AK_ReceptionAwareness_RefToOutMessageId", x => x.RefToOutMessageId);
                    table.ForeignKey(
                        name: "FK_OutMessages_Id_ReceptionAwareness_RefToOutMessageId",
                        column: x => x.RefToOutMessageId,
                        principalColumn: "Id",
                        principalTable: "OutMessages",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("INSERT INTO ReceptionAwareness " +
                                 "(CurrentRetryCount, InsertionTime, RefToOutMessageId, RefToEbmsMessageId, " +
                                 " LastSendTime, ModificationTime, RetryInterval, " +
                                 " Status, TotalRetryCount) " +
                                 "SELECT r.CurrentRetryCount, r.InsertionTime, o.Id, r.InternalMessageId, " +
                                 " r.LastSendTime, r.ModificationTime, r.RetryInterval, " +
                                 " r.Status, r.TotalRetryCount " +
                                 "FROM OldReceptionAwareness r " +
                                 "INNER JOIN OutMessages o ON o.EbmsMessageId = r.InternalMessageId");

            migrationBuilder.DropTable("OldReceptionAwareness");

            migrationBuilder.CreateIndex(
                name: "IX_ReceptionAwareness_Status_CurrentRetryCount",
                table: "ReceptionAwareness",
                columns: new[] { "Status", "CurrentRetryCount" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
           // TODO: downgrade code.
        }
    }
}
