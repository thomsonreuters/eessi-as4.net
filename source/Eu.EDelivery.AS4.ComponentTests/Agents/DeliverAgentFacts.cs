using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Xunit;
using static Eu.EDelivery.AS4.ComponentTests.Properties.Resources;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class DeliverAgentFacts : ComponentTestTemplate
    {
        private const string ContentType =
            "multipart/related; boundary=\"MIMEBoundary_18bd76d83b2fa5adb6f4e198ff24bcc40fcdb2988035bd08\"; type=\"application/soap+xml\"; charset=\"utf-8\"";

        private readonly AS4Component _as4Msh;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeliverAgentFacts"/> class.
        /// </summary>
        public DeliverAgentFacts()
        {
            OverrideSettings("deliveragent_http_settings.xml");
            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);
        }

        [Fact]
        public async Task StatusIsSetToException_IfDeliveryFails()
        {
            // Arrange
            AS4Message as4Message = await CreateSinglePayloadMessage();

            string deliverLocation = DeliverLocationOf(as4Message);
            CleanDirectoryAt(deliverLocation);

            using (WriteBlockingFileTo(deliverLocation))
            {
                // Act
                await InsertToBeDeliveredMessage(as4Message);

                // Assert
                InMessage actualMessage = GetToBeDeliveredMessage(as4Message);
                Assert.Equal(InStatus.Exception, actualMessage.Status);
            }
        }

        private static async Task<AS4Message> CreateSinglePayloadMessage()
        {
            var serializer = new MimeMessageSerializer(new SoapEnvelopeSerializer());
            return await serializer.DeserializeAsync(new MemoryStream(deliveragent_message), ContentType, CancellationToken.None);
        }

        private static void CleanDirectoryAt(string location)
        {
            string directoryName = Path.GetDirectoryName(location);
            Assert.NotNull(directoryName);

            foreach (FileInfo file in new DirectoryInfo(directoryName).EnumerateFiles())
            {
                file.Delete();
            }
        }

        private static IDisposable WriteBlockingFileTo(string deliverLocation)
        {
            var fileStream = new FileStream(deliverLocation, FileMode.CreateNew);
            var streamWriter = new StreamWriter(fileStream);

            streamWriter.Write("<blocking content>");

            return streamWriter;
        }

        private static string DeliverLocationOf(AS4Message as4Message)
        {
            return Path.Combine(Environment.CurrentDirectory, @"messages\in", as4Message.GetPrimaryMessageId() + ".xml");
        }

        private async Task InsertToBeDeliveredMessage(AS4Message as4Message)
        {
            var context = new DatastoreContext(_as4Msh.GetConfiguration());
            var repository = new DatastoreRepository(context);
            repository.InsertInMessage(await CreateInMessageFrom(as4Message));
            await context.SaveChangesAsync();
        }

        private InMessage GetToBeDeliveredMessage(AS4Message as4Message)
        {
            // Wait till the AS4 Component had updated the record
            Thread.Sleep(TimeSpan.FromSeconds(5));

            var spy = new DatabaseSpy(_as4Msh.GetConfiguration());
            return spy.GetInMessageFor(m => m.EbmsMessageId.Equals(as4Message.GetPrimaryMessageId()));
        }

        private static async Task<InMessage> CreateInMessageFrom(AS4Message as4Message)
        {
           return new InMessage
            {
                ContentType = as4Message.ContentType,
                EbmsMessageId = as4Message.GetPrimaryMessageId(),
                EbmsMessageType = MessageType.UserMessage,
                MEP = MessageExchangePattern.Push,
                MessageLocation =
                    await Config.Instance.AS4MessageBodyPersister.SaveAS4MessageAsync(Config.Instance.InStore, as4Message, CancellationToken.None),
                PMode = deliveragent_pmode, 
                Operation = Operation.ToBeDelivered
            };
        }

        protected override void Disposing(bool isDisposing)
        {
            _as4Msh.Dispose();
        }
    }
}
