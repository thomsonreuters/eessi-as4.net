using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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
using Eu.EDelivery.AS4.UnitTests.Common;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;

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
            public void HasDefaultOperation()
            {
                Assert.Equal(Operation.NotApplicable, new StubMessageEntity().Operation);
            }

            [Fact]
            public void HasDefaultMessageExchangePattern()
            {
                Assert.Equal(MessageExchangePattern.Push, new StubMessageEntity().MEP);
            }

            [Fact]
            public void HasDefaultMessageType()
            {
                Assert.Equal(MessageType.UserMessage, new StubMessageEntity().EbmsMessageType);
            }

            [Fact]
            public void GetsPartyInfoFromEntity()
            {
                // Arrange
                AS4Message expected = CreateAS4MessageWithUserMessage();

                // Act
                MessageEntity actual = BuildForMessageUnit(expected.FirstUserMessage);

                // Assert
                MessageEntityAssertion.AssertPartyInfo(expected, actual);
            }

            [Fact]
            public void GetsCollaborationInfo()
            {
                // Arrange
                AS4Message expected = CreateAS4MessageWithUserMessage();

                // Act
                MessageEntity actual = BuildForMessageUnit(expected.FirstUserMessage);

                // Assert
                MessageEntityAssertion.AssertCollaborationInfo(expected, actual);
            }

            [Fact]
            public void GetsMetaInfo_ForUserMessage()
            {
                // Arrange
                AS4Message expected = CreateAS4MessageWithUserMessage();

                // Act
                MessageEntity actual = BuildForMessageUnit(expected.FirstUserMessage);

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
                MessageEntity actual = BuildForMessageUnit(expected.FirstSignalMessage);

                // Assert
                MessageEntityAssertion.AssertSignalMessageMetaInfo(expected, actual);

            }

            [Fact]
            public void GetsSoapEnvelope()
            {
                // Arrange
                AS4Message expected = CreateAS4MessageWithUserMessage();

                // Act
                MessageEntity actual = BuildForMessageUnit(expected.FirstUserMessage);

                // Assert
                MessageEntityAssertion.AssertSoapEnvelope(expected.FirstUserMessage, actual);
            }

            private static AS4Message CreateAS4MessageWithUserMessage()
            {
                return AS4Message.Create(new FilledUserMessage());
            }

            private static AS4Message CreateAS4MessageWithReceiptMessage(bool isDuplicate)
            {
                return AS4Message.Create(new FilledNRReceipt { IsDuplicate = isDuplicate });
            }

            private static MessageEntity BuildForMessageUnit(MessageUnit expected)
            {
                var message = new StubMessageEntity();
                message.AssignAS4Properties(expected);

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
                var sut = new StubMessageEntity();
                sut.Operation = expectedOperation;

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

        public class Persistence : GivenDatastoreFacts
        {
            [Fact]
            public async Task IdIsCorrectlyRetrieved()
            {
                const string messageId = "messageId";

                using (var db = GetDataStoreContext())
                {
                    var inMessage = new InMessage(messageId) { MessageLocation = "test" };

                    Assert.Equal(default(int), inMessage.Id);

                    db.InMessages.Add(inMessage);

                    await db.SaveChangesAsync();
                }

                using (var db = GetDataStoreContext())
                {
                    var inMessage = db.InMessages.FirstOrDefault(m => m.EbmsMessageId == messageId);
                    Assert.NotNull(inMessage);
                    Assert.NotEqual(default(int), inMessage.Id);
                }
            }

            [Fact]
            public async Task PModeInformationIsCorrectlyRetrieved()
            {
                const string messageId = "messageId";
                const string pmodeId = "TestPModeId";

                using (var db = GetDataStoreContext())
                {
                    var inMessage = new InMessage(messageId) { MessageLocation = "test" };
                    inMessage.SetPModeInformation(new SendingProcessingMode() { Id = pmodeId });

                    db.InMessages.Add(inMessage);

                    await db.SaveChangesAsync();
                }

                using (var db = GetDataStoreContext())
                {
                    var inMessage = db.InMessages.FirstOrDefault(m => m.EbmsMessageId == messageId);
                    Assert.NotNull(inMessage);
                    Assert.False(String.IsNullOrWhiteSpace(inMessage.PModeId));
                    Assert.False(String.IsNullOrWhiteSpace(inMessage.PMode));
                }
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
                using (Stream actualStream = await sut.RetrieveMessageBody(store: null))
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
                using (Stream actualStream = await sut.RetrieveMessageBody(stubProvider))
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
        }
    }
}