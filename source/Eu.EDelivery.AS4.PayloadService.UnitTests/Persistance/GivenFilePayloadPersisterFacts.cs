using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.PayloadService.Models;
using Eu.EDelivery.AS4.PayloadService.Persistance;
using Eu.EDelivery.AS4.PayloadService.UnitTests.Models;
using Eu.EDelivery.AS4.PayloadService.UnitTests.Serialization;
using Xunit;

namespace Eu.EDelivery.AS4.PayloadService.UnitTests.Persistance
{
    /// <summary>
    /// Testing <see cref="FilePayloadPersister"/>
    /// </summary>
    public class GivenFilePayloadPersisterFacts
    {
        [Fact]
        public async Task WritesFileWithMetaToDisk()
        {
            // Arrange
            const string expectedContent = "message data!";
            using (Stream serializeContent = expectedContent.AsStream())
            {
                var persister = new FilePayloadPersister(new CurrentDirectoryHostingEnvironment());

                // Act
                var payload = new Payload(serializeContent, CreateUniquePayloadMeta());
                string newPayloadId = await persister.SavePayload(payload);

                // Assert
                Assert.Equal(expectedContent, DeserializeContent(newPayloadId));
                Assert.Contains("originalfilename:", DeserializeContent(newPayloadId + ".meta"));
            }
        }

        [Fact]
        public async Task LoadsPayloadWithMetaFromDisk()
        {
            // Arrange
            const string expectedContent = "message data!";
            using (Stream serializeContent = expectedContent.AsStream())
            {
                var persister = new FilePayloadPersister(new CurrentDirectoryHostingEnvironment());
                var payload = new Payload(serializeContent, CreateUniquePayloadMeta());
                string savedPayloadId = await persister.SavePayload(payload);

                // Act
                using (Payload actualPayload = await persister.LoadPayload(savedPayloadId))
                {
                    Assert.Equal(expectedContent, actualPayload.DeserializeContent());
                }
            }
        }

        private static PayloadMeta CreateUniquePayloadMeta()
        {
            return new PayloadMeta(Guid.NewGuid() + ".txt");
        }

        private static string DeserializeContent(string id)
        {
            return File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Payloads", id));
        }
    }
}