using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Eu.EDelivery.AS4.Migrations
{
    public partial class AddSmpConfigurationTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SmpConfigurations",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Action = table.Column<string>(maxLength: 255, nullable: true),
                    EncryptAlgorithm = table.Column<string>(nullable: true),
                    EncryptAlgorithmKeySize = table.Column<int>(nullable: false),
                    EncryptKeyDigestAlgorithm = table.Column<string>(nullable: true),
                    EncryptKeyMgfAlorithm = table.Column<string>(nullable: true),
                    EncryptKeyTransportAlgorithm = table.Column<string>(nullable: true),
                    EncryptPublicKeyCertificate = table.Column<string>(nullable: true),
                    EncryptionEnabled = table.Column<bool>(nullable: false),
                    FinalRecipient = table.Column<string>(nullable: true),
                    PartyRole = table.Column<string>(maxLength: 255, nullable: false),
                    PartyType = table.Column<string>(maxLength: 255, nullable: false),
                    ServiceType = table.Column<string>(maxLength: 255, nullable: true),
                    ServiceValue = table.Column<string>(maxLength: 255, nullable: true),
                    TLSEnabled = table.Column<bool>(nullable: false),
                    ToPartyId = table.Column<string>(maxLength: 255, nullable: false),
                    URL = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmpConfigurations", x => x.Id);
                    table.UniqueConstraint("AK_SmpConfigurations_ToPartyId_PartyRole_PartyType", x => new { x.ToPartyId, x.PartyRole, x.PartyType });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SmpConfigurations");
        }
    }
}
