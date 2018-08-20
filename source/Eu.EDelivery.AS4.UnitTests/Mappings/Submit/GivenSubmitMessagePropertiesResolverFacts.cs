using System.Collections.Generic;
using Eu.EDelivery.AS4.Mappings.Submit;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Serialization;
using Xunit;
using MessageProperty = Eu.EDelivery.AS4.Model.Core.MessageProperty;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Submit
{
    /// <summary>
    /// Testing <see cref="SubmitMessagePropertiesResolver" />
    /// </summary>
    public class GivenSubmitMessagePropertiesResolverFacts
    {
        public class GivenValidArguments : GivenSubmitMessagePropertiesResolverFacts
        {
            [Fact]
            public void ThenResolverSubmitMessagePropertiesFromSubmitMessageAndPMode()
            {
                // Arrange
                SubmitMessage submitMessage = CreatePopulatedSubmitMessage();
                SendingProcessingMode pmode = CreateaPopulatedSendingPMode();
                AS4.Model.PMode.MessageProperty pmodeProperty = CreatePopulatedMessageProperty();
                pmode.MessagePackaging.MessageProperties = new List<AS4.Model.PMode.MessageProperty> { pmodeProperty };
                submitMessage.PMode = pmode;

                // Act
                MessageProperty[] properties = SubmitMessagePropertiesResolver.Default.Resolve(submitMessage);

                // Assert
                Assert.Equal(2, properties.Length);
            }

            private static AS4.Model.PMode.MessageProperty CreatePopulatedMessageProperty()
            {
                return new AS4.Model.PMode.MessageProperty
                {
                    Name = "pmode-name",
                    Value = "pmode-value",
                    Type = "pmode-type"
                };
            }

            [Fact]
            public void ThenResolveSubmitMessagePropertiesFromSubmitMessage()
            {
                // Arrange
                SubmitMessage submitMessage = CreatePopulatedSubmitMessage();
                submitMessage.PMode = CreateaPopulatedSendingPMode();

                // Act
                MessageProperty[] properties = SubmitMessagePropertiesResolver.Default.Resolve(submitMessage);

                // Assert
                Assert.Single(properties);
            }

            private static SubmitMessage CreatePopulatedSubmitMessage()
            {
                return AS4XmlSerializer.FromString<SubmitMessage>(Properties.Resources.submitmessage);
            }

            private static SendingProcessingMode CreateaPopulatedSendingPMode()
            {
                return AS4XmlSerializer.FromString<SendingProcessingMode>(Properties.Resources.sendingprocessingmode);
            }
        }
    }
}