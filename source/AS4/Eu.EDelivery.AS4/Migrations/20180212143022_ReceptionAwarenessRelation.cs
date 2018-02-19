using System;
using Eu.EDelivery.AS4.Common;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Eu.EDelivery.AS4.Migrations
{
    [DbContext(typeof(DatastoreContext))]
    [Migration("ReceptionAwarenessRelation")]
    public class ReceptionAwarenessRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider.Equals(
                "Microsoft.EntityFrameworkCore.Sqlite",
                StringComparison.OrdinalIgnoreCase))
            {
                DropAndRecreateReceptionAwarenessTable(migrationBuilder);
            }
            else
            {
                AlterReceptionAwarenessTable(migrationBuilder);
            }
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
                    table.ForeignKey(
                        name: "FK_OutMessages_EbmsMessageId_ReceptionAwareness_InternalMessageId",
                        column: x => x.InternalMessageId,
                        principalColumn: "EbmsMessageId",
                        principalTable: "OutMessages",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("INSERT INTO ReceptionAwareness " +
                                 "(CurrentRetryCount, InsertionTime, InternalMessageId, " +
                                 " LastSendTime, ModificationTime, RetryInterval, " +
                                 " Status, TotalRetryCount) " +
                                 "SELECT r.CurrentRetryCount, r.InsertionTime, r.InternalMessageId, " +
                                 " r.LastSendTime, r.ModificationTime, r.RetryInterval, " +
                                 " r.Status, r.TotalRetryCount " +
                                 "FROM OldReceptionAwareness r " +
                                 "INNER JOIN OutMessages ON OutMessages.EbmsMessageId = r.InternalMessageId");

            migrationBuilder.DropTable("OldReceptionAwareness");

            migrationBuilder.CreateIndex(
                name: "IX_ReceptionAwareness_Status_CurrentRetryCount",
                table: "ReceptionAwareness",
                columns: new[] { "Status", "CurrentRetryCount" });
        }

        private static void AlterReceptionAwarenessTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                table: "ReceptionAwareness",
                name: "FK_OutMessages_EbmsMessageId_ReceptionAwareness_InternalMessageId",
                column: "InternalMessageId",
                principalColumn: "EbmsMessageId",
                principalTable: "OutMessages",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider.Equals(
                "Microsoft.EntityFrameworkCore.Sqlite",
                StringComparison.OrdinalIgnoreCase))
            {
                RecreatePreviousReceptionAwarenessTable(migrationBuilder);
            }
            else
            {
                RestoreReceptionAwarenessTable(migrationBuilder);
            }
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

        private static void RestoreReceptionAwarenessTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                table: "ReceptionAwareness",
                name: "FK_OutMessages_EbmsMessageId_ReceptionAwareness_InternalMessageId");

        }
    }
}
