using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.UnitTests.Builders.Entities;
using Eu.EDelivery.AS4.UnitTests.Model;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Entities
{
    /// <summary>
    /// Testing <see cref="MessageEntity"/>
    /// </summary>
    public class GivenMessageEntityFacts
    {
        public class Create
        {
            [Fact]
            public void GetsPartyInfoFromEntity()
            {
                // Arrange
                AS4Message expected = CreateAS4MessageWithUserMessage();

                // Act
                MessageEntity actual = BuildForMessageUnit(expected.PrimaryUserMessage);

                // Assert
                MessageEntityAssertion.AssertPartyInfo(expected, actual);
            }

            [Fact]
            public void GetsCollaborationInfo()
            {
                // Arrange
                AS4Message expected = CreateAS4MessageWithUserMessage();

                // Act
                MessageEntity actual = BuildForMessageUnit(expected.PrimaryUserMessage);

                // Assert
                MessageEntityAssertion.AssertCollaborationInfo(expected, actual);
            }

            [Fact]
            public void GetsMetaInfo_ForUserMessage()
            {
                // Arrange
                AS4Message expected = CreateAS4MessageWithUserMessage();

                // Act
                MessageEntity actual = BuildForMessageUnit(expected.PrimaryUserMessage);

                // Assert
                MessageEntityAssertion.AssertUserMessageMetaInfo(expected, actual);
            }

            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public void GetsMetaInfo_ForSignalMessage(bool isDuplicate)
            {
                // Arrange
                AS4Message expected = CreateAS4MessageWithReceiptMessage(isDuplicate: isDuplicate);

                // Act
                MessageEntity actual = BuildForMessageUnit(expected.PrimarySignalMessage);

                // Assert
                MessageEntityAssertion.AssertSignalMessageMetaInfo(expected, actual);

            }

            [Fact]
            public void GetsSoapEnvelope()
            {
                // Arrange
                AS4Message expected = CreateAS4MessageWithUserMessage();

                // Act
                MessageEntity actual = BuildForMessageUnit(expected.PrimaryUserMessage);

                // Assert
                MessageEntityAssertion.AssertSoapEnvelope(expected.PrimaryUserMessage, actual);
            }

            private static AS4Message CreateAS4MessageWithUserMessage()
            {
                return AS4Message.Create(new FilledUserMessage());
            }

            private static AS4Message CreateAS4MessageWithReceiptMessage(bool isDuplicate)
            {
                return AS4Message.Create(new FilledNRRReceipt { IsDuplicate = isDuplicate });
            }

            private static MessageEntity BuildForMessageUnit(MessageUnit expected)
            {
                var message = new StubMessageEntity();
                message.AssignAS4Properties(expected, CancellationToken.None);

                return message;
            }
        }

        public class Lock
        {
            [Fact]
            public void MessageEntityLocksInstanceByUpdatingOperation()
            {
                // Arrange
                var sut = new StubMessageEntity();
                const Operation expectedOperation = Operation.Sending;

                // Act
                sut.Lock(expectedOperation.ToString());

                // Assert
                Assert.Equal(Operation.Sending, sut.Operation);
            }

            [Fact]
            public void MessageEntityDoesntLockInstance_IfUpdateOperationIsNotApplicable()
            {
                // Arrange
                const Operation expectedOperation = Operation.Notified;
                var sut = new StubMessageEntity { Operation = expectedOperation };

                // Act
                sut.Lock(Operation.NotApplicable.ToString());

                // Assert
                Assert.Equal(expectedOperation, sut.Operation);
            }
        }

        public class PMode : GivenMessageEntityFacts
        {
            [Fact]
            public void SendingPModeInformationIsCorrectlySet()
            {
                var entity = new StubMessageEntity();

                var sendingPMode = new SendingProcessingMode() { Id = "sending_pmode_id" };

                entity.SetPModeInformation(sendingPMode);

                Assert.Equal(sendingPMode.Id, entity.PModeId);
                Assert.Equal(entity.PMode, AS4XmlSerializer.ToString(sendingPMode));
            }

            [Fact]
            public void ReceivingPModeInformationIsCorrectlySet()
            {
                var entity = new StubMessageEntity();

                var receivingPMode = new ReceivingProcessingMode() { Id = "sending_pmode_id" };

                entity.SetPModeInformation(receivingPMode);

                Assert.Equal(receivingPMode.Id, entity.PModeId);
                Assert.Equal(entity.PMode, AS4XmlSerializer.ToString(receivingPMode));

            }
        }

        public class RetrieveMessageBody
        {
            [Fact]
            public async Task MessageBodyReturnsNullStream_IfNoMessageLocationIsSpecified()
            {
                // Arrange
                StubMessageEntity sut = CreateMessageEntity(messageLocation: null);

                // Act
                using (Stream actualStream = await sut.RetrieveMessagesBody(store: null))
                {
                    // Assert
                    Assert.Null(actualStream);
                }
            }

            [Fact]
            public async Task MessageEntityCatchesInvalidMessageBodyRetrieval()
            {
                // Arrange
                StubMessageEntity sut = CreateMessageEntity(messageLocation: "ignored");
                var stubProvider = new MessageBodyStore();
                stubProvider.Accept(condition: s => true, persister: new SaboteurMessageBodyStore());

                // Act
                using (Stream actualStream = await sut.RetrieveMessagesBody(stubProvider))
                {
                    // Assert
                    Assert.Null(actualStream);
                }
            }

            private static StubMessageEntity CreateMessageEntity(string messageLocation)
            {
                return new StubMessageEntity { MessageLocation = messageLocation };
            }
        }

        [ExcludeFromCodeCoverage]
        private class StubMessageEntity : MessageEntity
        {
            public override string StatusString { get; set; }
        }
    }
}