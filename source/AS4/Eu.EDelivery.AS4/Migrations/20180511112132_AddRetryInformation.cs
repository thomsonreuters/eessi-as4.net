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
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxRetryCount",
                table: "OutMessages",
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RetryInterval",
                table: "OutMessages",
                maxLength: 50,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentRetryCount",
                table: "InMessages",
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxRetryCount",
                table: "InMessages",
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RetryInterval",
                table: "InMessages",
                maxLength: 50,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentRetryCount",
                table: "OutExceptions",
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxRetryCount",
                table: "OutExceptions",
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RetryInterval",
                table: "OutExceptions",
                maxLength: 50,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentRetryCount",
                table: "InExceptions",
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxRetryCount",
                table: "InExceptions",
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RetryInterval",
                table: "InExceptions",
                maxLength: 50,
                defaultValue: 0);
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

            migrationBuilder.DropColumn(
                name: "CurrentRetryCount",
                table: "OutExceptions");

            migrationBuilder.DropColumn(
                name: "MaxRetryCount",
                table: "OutExceptions");

            migrationBuilder.DropColumn(
                name: "RetryInterval",
                table: "OutExceptions");

            migrationBuilder.DropColumn(
                name: "CurrentRetryCount",
                table: "InExceptions");

            migrationBuilder.DropColumn(
                name: "MaxRetryCount",
                table: "InExceptions");

            migrationBuilder.DropColumn(
                name: "RetryInterval",
                table: "InExceptions");
        }
    }
}
