using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Eu.EDelivery.AS4.Migrations
{
    public partial class OptimizeColumnSizes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider.Equals("Microsoft.EntityFrameworkCore.Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                DropAndRecreateOutMessageTable(migrationBuilder);
                DropAndRecreateInMessageTable(migrationBuilder);
                DropAndRecreateOutExceptionTable(migrationBuilder);
                DropAndRecreateInExceptionTable(migrationBuilder);
                DropAndRecreateReceptionAwarenessTable(migrationBuilder);
            }
            else
            {
                AlterOutMessageTable(migrationBuilder);
                AlterInMessageTable(migrationBuilder);
                AlterOutExceptionTable(migrationBuilder);
                AlterInExceptionTable(migrationBuilder);
                AlterReceptionAwarenessTable(migrationBuilder);
            }
        }

        private static void AlterReceptionAwarenessTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint("AK_ReceptionAwareness_InternalMessageId", "ReceptionAwareness");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ReceptionAwareness",
                maxLength: 25,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RetryInterval",
                table: "ReceptionAwareness",
                maxLength: 12,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InternalMessageId",
                table: "ReceptionAwareness",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AddUniqueConstraint("AK_ReceptionAwareness_InternalMessageId", "ReceptionAwareness", "InternalMessageId");
        }

        private static void AlterInExceptionTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PModeId",
                table: "InExceptions",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EbmsRefToMessageId",
                table: "InExceptions",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }

        private static void AlterOutExceptionTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
               name: "PModeId",
               table: "OutExceptions",
               maxLength: 256,
               nullable: true,
               oldClrType: typeof(string),
               oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EbmsRefToMessageId",
                table: "OutExceptions",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }

        private static void AlterInMessageTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "InMessages",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PModeId",
                table: "InMessages",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EbmsRefToMessageId",
                table: "InMessages",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EbmsMessageType",
                table: "InMessages",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EbmsMessageId",
                table: "InMessages",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }

        private static void AlterOutMessageTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint("AK_OutMessages_EbmsMessageId", "OutMessages");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "OutMessages",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PModeId",
                table: "OutMessages",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EbmsRefToMessageId",
                table: "OutMessages",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EbmsMessageType",
                table: "OutMessages",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EbmsMessageId",
                table: "OutMessages",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AddUniqueConstraint("AK_OutMessages_EbmsMessageId", "OutMessages", "EbmsMessageId");
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
                    table.UniqueConstraint("AK_OutMessages_EbmsMessageId", x => x.EbmsMessageId);
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

        private static void DropAndRecreateInMessageTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("InMessages", newName: "OldInMessages");

            migrationBuilder.CreateTable(
                name: "InMessages",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),

                    Action = table.Column<string>(maxLength: 255, nullable: true),
                    ContentType = table.Column<string>(maxLength: 256, nullable: true),
                    ConversationId = table.Column<string>(maxLength: 50, nullable: true),
                    EbmsMessageId = table.Column<string>(maxLength: 256, nullable: true),
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
                    table.PrimaryKey("PK_InMessages", x => x.Id);
                });

            migrationBuilder.Sql("INSERT INTO InMessages " +
                                 "(Action, ContentType, ConversationId, EbmsMessageId, EbmsMessageType, " +
                                 " EbmsRefToMessageId, FromParty, InsertionTime, Intermediary, IsDuplicate, " +
                                 " IsTest, MEP, MessageLocation, ModificationTime, MPC, Operation, PMode, " +
                                 " PModeId, Service, SoapEnvelope, Status, ToParty) " +
                                 "SELECT Action, ContentType, ConversationId, EbmsMessageId, EbmsMessageType, " +
                                 " EbmsRefToMessageId, FromParty, InsertionTime, Intermediary, IsDuplicate, " +
                                 " IsTest, MEP, MessageLocation, ModificationTime, MPC, Operation, PMode, " +
                                 " PModeId, Service, SoapEnvelope, Status, ToParty " +
                                 "FROM OldInMessages", suppressTransaction: true);

            migrationBuilder.DropTable("OldInMessages");

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
                    InternalMessageId = table.Column<string>(maxLength: 256, nullable: false),
                    LastSendTime = table.Column<DateTimeOffset>(nullable: true),
                    ModificationTime = table.Column<DateTimeOffset>(nullable: false),
                    RetryInterval = table.Column<string>(maxLength: 12, nullable: true),
                    Status = table.Column<string>(maxLength: 25, nullable: true),
                    TotalRetryCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceptionAwareness", x => x.Id);
                    table.UniqueConstraint("AK_ReceptionAwareness_InternalMessageId", x => x.InternalMessageId);
                });

            migrationBuilder.Sql("INSERT INTO ReceptionAwareness " +
                                 "(CurrentRetryCount, InsertionTime, InternalMessageId, " +
                                 " LastSendTime, ModificationTime, RetryInterval, " +
                                 " Status, TotalRetryCount) " +
                                 "SELECT CurrentRetryCount, InsertionTime, InternalMessageId, " +
                                 " LastSendTime, ModificationTime, RetryInterval, " +
                                 " Status, TotalRetryCount " +
                                 "FROM OldReceptionAwareness");

            migrationBuilder.DropTable("OldReceptionAwareness");

            migrationBuilder.CreateIndex(
                name: "IX_ReceptionAwareness_Status_CurrentRetryCount",
                table: "ReceptionAwareness",
                columns: new[] { "Status", "CurrentRetryCount" });
        }

        private static void DropAndRecreateInExceptionTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("InExceptions", newName: "OldInExceptions");

            migrationBuilder.CreateTable(
                name: "InExceptions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                              .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EbmsRefToMessageId = table.Column<string>(maxLength: 256, nullable: true),
                    Exception = table.Column<string>(nullable: true),
                    InsertionTime = table.Column<DateTimeOffset>(nullable: false),
                    MessageBody = table.Column<byte[]>(nullable: true),
                    ModificationTime = table.Column<DateTimeOffset>(nullable: false),
                    Operation = table.Column<string>(maxLength: 50, nullable: true),
                    PMode = table.Column<string>(nullable: true),
                    PModeId = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InExceptions", x => x.Id);
                });

            migrationBuilder.Sql("INSERT INTO InExceptions" +
                                 "(EbmsRefToMessageId, Exception, InsertionTime, MessageBody, " +
                                 " ModificationTime, Operation, PMOde, PModeId) " +
                                 "SELECT EbmsRefToMessageId, Exception, InsertionTime, MessageBody, " +
                                 " ModificationTime, Operation, PMOde, PModeId " +
                                 "FROM OldInExceptions");

            migrationBuilder.DropTable("OldInExceptions");

            migrationBuilder.CreateIndex(
                name: "IX_InExceptions_EbmsRefToMessageId",
                table: "InExceptions",
                column: "EbmsRefToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_InExceptions_Operation",
                table: "InExceptions",
                column: "Operation");
        }

        private static void DropAndRecreateOutExceptionTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("OutExceptions", newName: "OldOutExceptions");

            migrationBuilder.CreateTable(
                name: "OutExceptions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                              .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EbmsRefToMessageId = table.Column<string>(maxLength: 256, nullable: true),
                    Exception = table.Column<string>(nullable: true),
                    InsertionTime = table.Column<DateTimeOffset>(nullable: false),
                    MessageBody = table.Column<byte[]>(nullable: true),
                    ModificationTime = table.Column<DateTimeOffset>(nullable: false),
                    Operation = table.Column<string>(maxLength: 50, nullable: true),
                    PMode = table.Column<string>(nullable: true),
                    PModeId = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutExceptions", x => x.Id);
                });

            migrationBuilder.Sql("INSERT INTO OutExceptions" +
                                 "(EbmsRefToMessageId, Exception, InsertionTime, MessageBody, " +
                                 " ModificationTime, Operation, PMOde, PModeId) " +
                                 "SELECT EbmsRefToMessageId, Exception, InsertionTime, MessageBody, " +
                                 " ModificationTime, Operation, PMOde, PModeId " +
                                 "FROM OldOutExceptions");

            migrationBuilder.DropTable("OldOutExceptions");

            migrationBuilder.CreateIndex(
                name: "IX_OutExceptions_EbmsRefToMessageId",
                table: "OutExceptions",
                column: "EbmsRefToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_OutExceptions_Operation",
                table: "OutExceptions",
                column: "Operation");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider.Equals("Microsoft.EntityFrameworkCore.Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                RecreatePreviousOutMessageTable(migrationBuilder);
                RecreatePreviousInMessageTable(migrationBuilder);
                RecreatePreviousOutExceptionTable(migrationBuilder);
                RecreatePreviousInExceptionTable(migrationBuilder);
                RecreatePreviousReceptionAwarenessTable(migrationBuilder);
            }
            else
            {
                RestoreReceptionAwarenessTable(migrationBuilder);
                RestoreOutMessagesTable(migrationBuilder);
                RestoreOutExceptionsTable(migrationBuilder);
                RestoreInMessagesTable(migrationBuilder);
                RestoreInExceptionsTable(migrationBuilder);
            }
        }

        private static void RestoreInExceptionsTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PModeId",
                table: "InExceptions",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EbmsRefToMessageId",
                table: "InExceptions",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 256,
                oldNullable: true);
        }

        private static void RestoreInMessagesTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "InMessages",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PModeId",
                table: "InMessages",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EbmsRefToMessageId",
                table: "InMessages",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EbmsMessageType",
                table: "InMessages",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EbmsMessageId",
                table: "InMessages",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 256,
                oldNullable: true);
        }

        private static void RestoreOutExceptionsTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PModeId",
                table: "OutExceptions",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EbmsRefToMessageId",
                table: "OutExceptions",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 256,
                oldNullable: true);
        }

        private static void RestoreOutMessagesTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint("AK_OutMessages_EbmsMessageId", "OutMessages");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "OutMessages",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PModeId",
                table: "OutMessages",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EbmsRefToMessageId",
                table: "OutMessages",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EbmsMessageType",
                table: "OutMessages",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EbmsMessageId",
                table: "OutMessages",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 256);

            migrationBuilder.AddUniqueConstraint("AK_OutMessages_EbmsMessageId", "OutMessages", "EbmsMessageId");
        }

        private static void RestoreReceptionAwarenessTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint("AK_ReceptionAwareness_InternalMessageId", "ReceptionAwareness");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ReceptionAwareness",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 25,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RetryInterval",
                table: "ReceptionAwareness",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 12,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InternalMessageId",
                table: "ReceptionAwareness",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 256);

            migrationBuilder.AddUniqueConstraint("AK_ReceptionAwareness_InternalMessageId", "ReceptionAwareness", "InternalMessageId");
        }

        private static void RecreatePreviousOutMessageTable(MigrationBuilder migrationBuilder)
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
                    EbmsMessageId = table.Column<string>(nullable: false),
                    EbmsMessageType = table.Column<string>(nullable: true),
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

        private static void RecreatePreviousInMessageTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("InMessages", newName: "OldInMessages");

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

            migrationBuilder.Sql("INSERT INTO InMessages " +
                                 "(Action, ContentType, ConversationId, EbmsMessageId, EbmsMessageType, " +
                                 " EbmsRefToMessageId, FromParty, InsertionTime, Intermediary, IsDuplicate, " +
                                 " IsTest, MEP, MessageLocation, ModificationTime, MPC, Operation, PMode, " +
                                 " PModeId, Service, SoapEnvelope, Status, ToParty) " +
                                 "SELECT Action, ContentType, ConversationId, EbmsMessageId, EbmsMessageType, " +
                                 " EbmsRefToMessageId, FromParty, InsertionTime, Intermediary, IsDuplicate, " +
                                 " IsTest, MEP, MessageLocation, ModificationTime, MPC, Operation, PMode, " +
                                 " PModeId, Service, SoapEnvelope, Status, ToParty " +
                                 "FROM OldInMessages", suppressTransaction: true);

            migrationBuilder.DropTable("OldInMessages");

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
        }

        private static void RecreatePreviousOutExceptionTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("OutExceptions", newName: "OldOutExceptions");

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

            migrationBuilder.Sql("INSERT INTO OutExceptions" +
                                 "(EbmsRefToMessageId, Exception, InsertionTime, MessageBody, " +
                                 " ModificationTime, Operation, PMOde, PModeId) " +
                                 "SELECT EbmsRefToMessageId, Exception, InsertionTime, MessageBody, " +
                                 " ModificationTime, Operation, PMOde, PModeId " +
                                 "FROM OldOutExceptions");

            migrationBuilder.DropTable("OldOutExceptions");

            migrationBuilder.CreateIndex(
                name: "IX_OutExceptions_EbmsRefToMessageId",
                table: "OutExceptions",
                column: "EbmsRefToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_OutExceptions_Operation",
                table: "OutExceptions",
                column: "Operation");
        }

        private static void RecreatePreviousInExceptionTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("InExceptions", newName: "OldInExceptions");

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

            migrationBuilder.Sql("INSERT INTO InExceptions" +
                                 "(EbmsRefToMessageId, Exception, InsertionTime, MessageBody, " +
                                 " ModificationTime, Operation, PMOde, PModeId) " +
                                 "SELECT EbmsRefToMessageId, Exception, InsertionTime, MessageBody, " +
                                 " ModificationTime, Operation, PMOde, PModeId " +
                                 "FROM OldInExceptions");

            migrationBuilder.DropTable("OldInExceptions");

            migrationBuilder.CreateIndex(
                name: "IX_InExceptions_EbmsRefToMessageId",
                table: "InExceptions",
                column: "EbmsRefToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_InExceptions_Operation",
                table: "InExceptions",
                column: "Operation");
        }

        private static void RecreatePreviousReceptionAwarenessTable(MigrationBuilder migrationBuilder)
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
                    InternalMessageId = table.Column<string>(nullable: false),
                    LastSendTime = table.Column<DateTimeOffset>(nullable: true),
                    ModificationTime = table.Column<DateTimeOffset>(nullable: false),
                    RetryInterval = table.Column<string>(nullable: true),
                    Status = table.Column<string>(maxLength: 450, nullable: true),
                    TotalRetryCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceptionAwareness", x => x.Id);
                    table.UniqueConstraint("AK_ReceptionAwareness_InternalMessageId", x => x.InternalMessageId);
                });

            migrationBuilder.Sql("INSERT INTO ReceptionAwareness " +
                                 "(CurrentRetryCount, InsertionTime, InternalMessageId, " +
                                 " LastSendTime, ModificationTime, RetryInterval, " +
                                 " Status, TotalRetryCount) " +
                                 "SELECT CurrentRetryCount, InsertionTime, InternalMessageId, " +
                                 " LastSendTime, ModificationTime, RetryInterval, " +
                                 " Status, TotalRetryCount " +
                                 "FROM OldReceptionAwareness");

            migrationBuilder.DropTable("OldReceptionAwareness");

            migrationBuilder.CreateIndex(
                name: "IX_ReceptionAwareness_Status_CurrentRetryCount",
                table: "ReceptionAwareness",
                columns: new[] { "Status", "CurrentRetryCount" });
        }
    }
}
