using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace Eu.EDelivery.AS4.Migrations
{
    public partial class ReplaceExceptionBodyWithLocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider.Equals("Microsoft.EntityFrameworkCore.Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                DropAndRecreateInExceptionTable(migrationBuilder);
                DropAndRecreateOutExceptionTable(migrationBuilder);
            }
            else
            {
                migrationBuilder.DropColumn(
                    name: "MessageBody",
                    table: "OutExceptions");

                migrationBuilder.DropColumn(
                    name: "MessageBody",
                    table: "InExceptions");

                migrationBuilder.AddColumn<string>(
                    name: "MessageLocation",
                    table: "OutExceptions",
                    nullable: true);

                migrationBuilder.AddColumn<string>(
                    name: "MessageLocation",
                    table: "InExceptions",
                    nullable: true);
            }
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
                    EbmsRefToMessageId = table.Column<string>(nullable: true, maxLength: 256),
                    Exception = table.Column<string>(nullable: true),
                    InsertionTime = table.Column<DateTimeOffset>(nullable: false),
                    MessageLocation = table.Column<string>(nullable: true),
                    ModificationTime = table.Column<DateTimeOffset>(nullable: false),
                    Operation = table.Column<string>(maxLength: 50, nullable: true),
                    PMode = table.Column<string>(nullable: true),
                    PModeId = table.Column<string>(nullable: true, maxLength: 256)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InExceptions", x => x.Id);
                });

            migrationBuilder.Sql(
                "INSERT INTO InExceptions "
                + "(EbmsRefToMessageId, Exception, InsertionTime, ModificationTime, Operation, PMode, PModeId) "
                + "SELECT EbmsRefToMessageId, Exception, InsertionTime, ModificationTime, Operation, PMode, PModeId "
                + "FROM OldInExceptions");

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
                    MessageLocation = table.Column<string>(nullable: true),
                    ModificationTime = table.Column<DateTimeOffset>(nullable: false),
                    Operation = table.Column<string>(maxLength: 50, nullable: true),
                    PMode = table.Column<string>(nullable: true),
                    PModeId = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutExceptions", x => x.Id);
                });

            migrationBuilder.Sql(
                "INSERT INTO OutExceptions "
                + "(EbmsRefToMessageId, Exception, InsertionTime, ModificationTime, Operation, PMode, PModeId) "
                + "SELECT EbmsRefToMessageId, Exception, InsertionTime, ModificationTime, Operation, PMode, PModeId "
                + "FROM OldOutExceptions");

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
                RecreatePreviousInExceptionTable(migrationBuilder);
                RecreatePreviousOutExceptionTable(migrationBuilder);
            }
           else
            {
                migrationBuilder.DropColumn(
                   name: "MessageLocation",
                   table: "OutExceptions");

                migrationBuilder.DropColumn(
                    name: "MessageLocation",
                    table: "InExceptions");

                migrationBuilder.AddColumn<byte[]>(
                    name: "MessageBody",
                    table: "OutExceptions",
                    nullable: true);

                migrationBuilder.AddColumn<byte[]>(
                    name: "MessageBody",
                    table: "InExceptions",
                    nullable: true); 
            }
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
                    EbmsRefToMessageId = table.Column<string>(nullable: true, maxLength: 256),
                    Exception = table.Column<string>(nullable: true),
                    InsertionTime = table.Column<DateTimeOffset>(nullable: false),
                    MessageBody = table.Column<byte[]>(nullable: true),
                    ModificationTime = table.Column<DateTimeOffset>(nullable: false),
                    Operation = table.Column<string>(maxLength: 50, nullable: true),
                    PMode = table.Column<string>(nullable: true),
                    PModeId = table.Column<string>(nullable: true, maxLength: 256)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InExceptions", x => x.Id);
                });

            migrationBuilder.Sql(
                "INSERT INTO InExceptions "
                + "(EbmsRefToMessageId, Exception, InsertionTime,  ModificationTime, Operation, PMode, PModeId) "
                + "SELECT EbmsRefToMessageId, Exception, InsertionTime, ModificationTime, Operation, PMode, PModeId "
                + "FROM OldInExceptions");

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

        private static void RecreatePreviousOutExceptionTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("OutExceptions", newName: "OldOutExceptions");

            migrationBuilder.CreateTable(
                name: "OutExceptions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                              .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EbmsRefToMessageId = table.Column<string>(nullable: true, maxLength: 256),
                    Exception = table.Column<string>(nullable: true),
                    InsertionTime = table.Column<DateTimeOffset>(nullable: false),
                    MessageBody = table.Column<byte[]>(nullable: true),
                    ModificationTime = table.Column<DateTimeOffset>(nullable: false),
                    Operation = table.Column<string>(maxLength: 50, nullable: true),
                    PMode = table.Column<string>(nullable: true),
                    PModeId = table.Column<string>(nullable: true, maxLength: 256)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutExceptions", x => x.Id);
                });

            migrationBuilder.Sql(
                "INSERT INTO OutExceptions "
                + "(EbmsRefToMessageId, Exception, InsertionTime, ModificationTime, Operation, PMode, PModeId) "
                + "SELECT EbmsRefToMessageId, Exception, InsertionTime, ModificationTime, Operation, PMode, PModeId "
                + "FROM OldOutExceptions");

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
    }
}
