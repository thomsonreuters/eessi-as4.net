using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Mappings.Common;
using Eu.EDelivery.AS4.Mappings.Submit;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.UnitTests.Common;
using CommonSchema = Eu.EDelivery.AS4.Model.Common.Schema;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Submit
{
    /// <summary>
    /// Testing <see cref="SubmitPayloadInfoResolver"/>
    /// </summary>
    public class GivenSubmitPayloadInfoResolverFacts
    {
        public GivenSubmitPayloadInfoResolverFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
            MapInitialization.InitializeMapper();
        }

        public class GivenValidArguments : GivenSubmitPayloadInfoResolverFacts
        {
            [Fact]
            public void ThenResolvePayloadInfoSucceeds()
            {
                // Arrange
                SubmitMessage submitMessage = base.CreatePopulatedSubmitMessage();
                submitMessage.PMode = base.CreatePopulatedSendingPMode();
                // Act
                List<PartInfo> partInfos = new SubmitPayloadInfoResolver().Resolve(submitMessage);
                // Assert
                Assert.Equal(2, partInfos.Count);
            }

            [Fact]
            public void ThenResolvePayloadInfoSucceedsWithPrefixedId()
            {
                // Arrange
                SubmitMessage submitMessage = base.CreatePopulatedSubmitMessage();
                submitMessage.PMode = base.CreatePopulatedSendingPMode();
                // Act
                List<PartInfo> partInfos = new SubmitPayloadInfoResolver().Resolve(submitMessage);
                // Assert
                partInfos.ForEach(p => Assert.StartsWith("cid:", p.Href));
            }

            [Fact]
            public void ThenResolverPayloadInfoSucceedsWithConfiguredCompressedPayloads()
            {
                // Arrange
                SubmitMessage submitMessage = base.CreatePopulatedSubmitMessage();
                submitMessage.PMode = base.CreatePopulatedSendingPMode();
                submitMessage.PMode.MessagePackaging.UseAS4Compression = true;
                // Act
                List<PartInfo> partInfos = new SubmitPayloadInfoResolver().Resolve(submitMessage);
                // Assert
                IEnumerable<PartInfo> compressedPartInfos = partInfos
                    .Where(i => i.Properties.ContainsKey("CompressionType"));
                Assert.Equal(2, compressedPartInfos.Count());
            }
        }

        public class GivenInvalidArguments : GivenSubmitPayloadInfoResolverFacts
        {
            [Fact]
            public void ThenResolveFailsWithEmptySchemaLocation()
            {
                // Arrange
                SubmitMessage submitMesage = base.CreatePopulatedSubmitMessage();
                submitMesage.Payloads.ForEach(p => p.Schemas = new[] {new CommonSchema(location: string.Empty)});
                submitMesage.PMode = base.CreatePopulatedSendingPMode();
                // Act / Assert
                Assert.Throws<AS4Exception>(
                    () => new SubmitPayloadInfoResolver().Resolve(submitMesage));
            }

            [Fact]
            public void ThenResolveFailsWithEmptyPayloadPropertyName()
            {
                // Arrange
                SubmitMessage submitMessage = base.CreatePopulatedSubmitMessage();
                submitMessage.Payloads.ForEach(
                    p => p.PayloadProperties = new[] {new PayloadProperty(name: string.Empty)});
                submitMessage.PMode = base.CreatePopulatedSendingPMode();
                // Act
                Assert.Throws<AS4Exception>(
                    () => new SubmitPayloadInfoResolver().Resolve(submitMessage));
            }
        }

        protected SubmitMessage CreatePopulatedSubmitMessage()
        {
            return AS4XmlSerializer.Deserialize<SubmitMessage>(Properties.Resources.submitmessage);
        }

        protected SendingProcessingMode CreatePopulatedSendingPMode()
        {
            return AS4XmlSerializer.Deserialize<SendingProcessingMode>(Properties.Resources.sendingprocessingmode);
        }
    }
}