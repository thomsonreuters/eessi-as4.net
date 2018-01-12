using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Mappings.Submit;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Submit
{
    /// <summary>
    /// Testing <see cref="SubmitPayloadInfoResolver" />
    /// </summary>
    public class GivenSubmitPayloadInfoResolverFacts
    {
        public GivenSubmitPayloadInfoResolverFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Default);
        }

        public class GivenValidArguments : GivenSubmitPayloadInfoResolverFacts
        {
            [Fact]
            public void ResolveEmptyPartInfos_WhenNoPayloadsArePresent()
            {
                // Arrange
                SubmitMessage message = CreatePopulatedSubmitMessage();
                message.Payloads = null;

                // Act
                List<PartInfo> actual = ExerciseResolve(message);

                // Assert
                Assert.Empty(actual);
            }

            [Fact]
            public void ThenResolvePayloadInfoSucceeds()
            {
                // Arrange
                SubmitMessage submitMessage = CreatePopulatedSubmitMessage();
                submitMessage.PMode = CreatePopulatedSendingPMode();

                // Act
                List<PartInfo> partInfos = ExerciseResolve(submitMessage);

                // Assert
                Assert.Equal(2, partInfos.Count);
                Assert.All(partInfos, p => Assert.DoesNotContain("CompressionType", p.Properties.Keys));
            }

            [Fact]
            public void ThenResolvePayloadInfoSucceedsWithPrefixedId()
            {
                // Arrange
                SubmitMessage submitMessage = CreatePopulatedSubmitMessage();
                submitMessage.PMode = CreatePopulatedSendingPMode();

                // Act
                List<PartInfo> partInfos = ExerciseResolve(submitMessage);

                // Assert
                partInfos.ForEach(p => Assert.StartsWith("cid:", p.Href));
            }

            [Fact]
            public void ThenResolverPayloadInfoSucceedsWithConfiguredCompressedPayloads()
            {
                // Arrange
                SubmitMessage submitMessage = CreatePopulatedSubmitMessage();
                submitMessage.PMode = CreatePopulatedSendingPMode();
                submitMessage.PMode.MessagePackaging.UseAS4Compression = true;

                // Act
                List<PartInfo> partInfos = ExerciseResolve(submitMessage);

                // Assert
                IEnumerable<PartInfo> compressedPartInfos =
                    partInfos.Where(i => i.Properties.ContainsKey("CompressionType"));
                Assert.Equal(2, compressedPartInfos.Count());
            }
        }

        protected SubmitMessage CreatePopulatedSubmitMessage()
        {
            return AS4XmlSerializer.FromString<SubmitMessage>(Properties.Resources.submitmessage);
        }

        protected SendingProcessingMode CreatePopulatedSendingPMode()
        {
            return AS4XmlSerializer.FromString<SendingProcessingMode>(Properties.Resources.sendingprocessingmode);
        }

        protected List<PartInfo> ExerciseResolve(SubmitMessage message)
        {
            var sut = SubmitPayloadInfoResolver.Default;

            return sut.Resolve(message);
        }
    }
}