using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Receivers.Specifications;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Receivers.Specifications
{
    /// <summary>
    /// Testing <see cref="ExpressionDatastoreSpecification"/>
    /// </summary>
    /// <seealso cref="GivenDatastoreFacts" />
    public class GivenExpressionDatastoreSpecificationFacts : GivenDatastoreFacts
    {
        [Theory]
        [InlineData("(Operation = ToBeDelivered")]
        [InlineData("Operation = ToBeDelivered MEP = Push")]
        [InlineData("Operation = ToBeDelivered ANDD MEP = Push")]
        [InlineData("Operation = ToBeDelivered AND OR MEP = Push")]
        [InlineData("Operation == ToBeDelivered")]
        [InlineData("Operation = ToBeDelivered = Push")]
        [InlineData("Operation (=) ToBeDelivered ()")]
        [InlineData("((( Operation )())) = ToBeDelivered )")]
        [InlineData("LK#$J#(@AND_)! J_!)I)##=)$)_I!@ _)#I$)I!)OR_)@$@")]
        [InlineData("(Operation) = ToBeDelivered")]
        [InlineData("Operation = To BeDelivered")]
        [InlineData("Operation !!= ToBeDelivered")]
        [InlineData("Operation ToBeDelivered")]
        [InlineData("Operation ISS NOT NULL")]
        [InlineData("Operation ISS NULL")]
        [InlineData("OperationISNULL")]
        [InlineData("OperationIS NOTNULL")]
        [InlineData("Operation IS = ToBeDelivered")]
        public void FailsToExpress_IfInvalidExpression(string filter)
        {
            // Arrange
            ExpressionDatastoreSpecification specification = CreateExpressionWith("InMessages", filter);
            InMessage expectedMessage = ExpectedMessage(Operation.ToBeDelivered, MessageExchangePattern.Push);

            // Act / Assert
            Assert.Throws<FormatException>(() => RunExpressionFor(specification, expectedMessage));
        }

        [Theory]
        [InlineData("Operation = ToBeDelivered AND MEP = Push")]
        [InlineData("(Operation = ToBeDelivered OR MEP = Push) AND EbmsMessageType = UserMessage")]
        [InlineData("(Operation = ToBeDelivered OR (MEP = Push AND EbmsMessageType != Receipt))")]
        [InlineData("Operation IS ToBeDelivered AND MEP = Push")]
        [InlineData("Operation IS ToBeDelivered OR (MEP = Push AND EbmsMessageType IS NOT Receipt)")]
        [InlineData("(Operation IS NOT Sending AND Operation IS ToBeDelivered)")]
        [InlineData("Operation = ToBeDelivered AND EbmsMessageId IS NOT NULL")]
        [InlineData("EbmsMessageId IS NOT NULL")]
        public void GetsExpectedInMessage_IfAndExpression(string filter)
        {
            // Arrange
            ExpressionDatastoreSpecification specification = CreateExpressionWith("InMessages", filter);
            InMessage expectedMessage = ExpectedMessage(Operation.ToBeDelivered, MessageExchangePattern.Push);

            // Act
            IEnumerable<Entity> actualMessages = RunExpressionFor(specification, expectedMessage);

            // Assert
            Assert.Equal(expectedMessage, actualMessages.First() as InMessage);
        }

        private static ExpressionDatastoreSpecification CreateExpressionWith(string table, string filter)
        {
            var args = new DatastoreSpecificationArgs(table, filter);
            var expression = new ExpressionDatastoreSpecification();
            expression.Configure(args);

            return expression;
        }

        private static InMessage ExpectedMessage(Operation operation, MessageExchangePattern pattern)
        {
            return new InMessage {EbmsMessageId = "message-id", Operation = operation, MEP = pattern};
        }

        private IEnumerable<Entity> RunExpressionFor(IDatastoreSpecification specification, InMessage expectedMessage)
        {
            using (DatastoreContext stubDatastore = CreateDatastoreWith(expectedMessage))
            {
                return specification.GetExpression().Compile()(stubDatastore).ToList();
            }
        }

        private DatastoreContext CreateDatastoreWith(InMessage expectedMessage)
        {
            var context = new DatastoreContext(Options);
            context.InMessages.Add(
                new InMessage {Operation = Operation.DeadLettered, EbmsMessageType = MessageType.Receipt});
            context.InMessages.Add(expectedMessage);
            context.SaveChanges();

            return context;
        }
    }
}