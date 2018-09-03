using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Strategies.Retriever;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Retriever
{
    public class GivenFilePayloadRetrieverFacts : PseudoConfig
    {
        [Fact]
        public async Task Retrieve_Payload_Fails_With_Invalid_FilePath()
        {
            await Assert.ThrowsAnyAsync<Exception>(
                () => ExerciseRetrieving("invalid-location"));
        }

        [Theory]
        [InlineData(@"config\settings.xml")]
        [InlineData(@"config\send-pmodes\pmode.xml")]
        [InlineData(@"config\receive-pmodes\pmode.xml")]
        public async Task Retrieve_Payload_Fails_With_Traversal_FilePath(string location)
        {
            await Assert.ThrowsAsync<NotSupportedException>(
                () => ExerciseRetrieving(location));
        }

        private async Task<Stream> ExerciseRetrieving(string location)
        {
            var sut = new FilePayloadRetriever(this);
            return await sut.RetrievePayloadAsync(location);
        }

        /// <summary>
        /// Gets the location where the payloads should be retrieved.
        /// </summary>
        public override string PayloadRetrievalLocation => @"\messages\attachments";
    }
}