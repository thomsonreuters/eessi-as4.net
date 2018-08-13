using Microsoft.EntityFrameworkCore.Migrations;

namespace Eu.EDelivery.AS4.Migrations
{
    public partial class RemoveReceptionAwarenessTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"INSERT INTO RetryReliability (RefToOutMessageId, RetryType, Status, CurrentRetryCount, MaxRetryCount, RetryInterval, LastRetryTime, InsertionTime, ModificationTime)
                  SELECT RefToOutMessageId, 'Send', Status, CurrentRetryCount, TotalRetryCount, RetryInterval, LastSendTime, InsertionTime, ModificationTime
                  FROM ReceptionAwareness");

            migrationBuilder.DropTable("ReceptionAwareness");
        }

        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
