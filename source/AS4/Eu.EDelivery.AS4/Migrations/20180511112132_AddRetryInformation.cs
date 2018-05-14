using Microsoft.EntityFrameworkCore.Migrations;

namespace Eu.EDelivery.AS4.Migrations
{
    public partial class AddRetryInformation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentRetryCount",
                table: "OutMessages",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxRetryCount",
                table: "OutMessages",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RetryInterval",
                table: "OutMessages",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentRetryCount",
                table: "InMessages",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxRetryCount",
                table: "InMessages",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RetryInterval",
                table: "InMessages",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentRetryCount",
                table: "OutMessages");

            migrationBuilder.DropColumn(
                name: "MaxRetryCount",
                table: "OutMessages");

            migrationBuilder.DropColumn(
                name: "RetryInterval",
                table: "OutMessages");

            migrationBuilder.DropColumn(
                name: "CurrentRetryCount",
                table: "InMessages");

            migrationBuilder.DropColumn(
                name: "MaxRetryCount",
                table: "InMessages");

            migrationBuilder.DropColumn(
                name: "RetryInterval",
                table: "InMessages");
        }
    }
}
