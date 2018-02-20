using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Strategies.Retriever;
using Eu.EDelivery.AS4.Transformers;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    public class GivenSubmitPayloadTransformerFacts
    {
        [Fact]
        public async Task TransformFromStreamToSinglePayload()
        {
            // Arrange
            var fixture = new ReceivedMessage(Stream.Null, "text/plain");

            // Act
            MessagingContext result = await ExerciseTransform(fixture);

            // Assert
            Assert.NotNull(result.SubmitMessage);
            Assert.Collection(result.SubmitMessage.Payloads, p =>
            {
                Assert.True(Guid.TryParse(p.Id, out Guid _));
                Assert.NotNull(new FileInfo(p.Location.Replace(TempFilePayloadRetriever.Key, string.Empty)));
                Assert.EndsWith(".txt", p.Location);
            });
        }

        [Fact]
        public async Task TransformFromFileStreamToSinglePayload()
        {
            // Arrange
            string payloadId = Guid.NewGuid().ToString();
            string payloadPath = Path.Combine(Path.GetTempPath(), payloadId + ".txt");

            var file = new FileStream(payloadPath, FileMode.Create);
            var fixture = new ReceivedMessage(file, "text/plain");

            // Act
            MessagingContext result = await ExerciseTransform(fixture);

            // Assert
            Assert.NotNull(result.SubmitMessage);
            Assert.Collection(result.SubmitMessage.Payloads, p =>
            {
                Assert.Equal(payloadId, p.Id);
                Assert.Equal(FilePayloadRetriever.Key + payloadPath, p.Location);
            });
        }

        private static async Task<MessagingContext> ExerciseTransform(ReceivedMessage msg)
        {
            const string pmodeId = "pmode-id";
            var stubConfig =
                new StubConfig(
                    new Dictionary<string, SendingProcessingMode> { [pmodeId] = new SendingProcessingMode() },
                    new Dictionary<string, ReceivingProcessingMode>());

            var sut = new SubmitPayloadTransformer(stubConfig);
            sut.Configure(new Dictionary<string, string> { ["SendingPMode"] = pmodeId });

            return await sut.TransformAsync(msg, CancellationToken.None);
        }
    }
}
