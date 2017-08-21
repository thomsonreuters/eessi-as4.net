using System;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Mappings.Core;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.Xml;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Core
{
    /// <summary>
    /// Testing <see cref="ErrorMap" />
    /// </summary>
    public class GivenErrorMapFacts
    {
        public GivenErrorMapFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Default);
        }

        public class GivenValidArguments : GivenErrorMapFacts
        {
            [Fact]
            public void ThenErrorDescriptionIsCorreclyMapped()
            {
                // Arrange
                string descriptionValue = Guid.NewGuid().ToString();
                string languageValue = Guid.NewGuid().ToString();

                SignalMessage signalMessage = GetPopulatedXmlError();
                signalMessage.Error[0].Description.Value = descriptionValue;
                signalMessage.Error[0].Description.lang = languageValue;

                // Act
                var error = AS4Mapper.Map<AS4.Model.Core.Error>(signalMessage);

                // Assert
                Assert.Equal(descriptionValue, error.Errors[0].Description.Value);
                Assert.Equal(languageValue, error.Errors[0].Description.Language);
            }

            [Fact]
            public void ThenMessageIdIsCorrectlyMapped()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                SignalMessage signalMessage = GetPopulatedXmlError();
                signalMessage.MessageInfo.MessageId = messageId;

                // Act
                var error = AS4Mapper.Map<AS4.Model.Core.Error>(signalMessage);

                // Assert
                Assert.Equal(messageId, error.MessageId);
            }
        }

        protected SignalMessage GetPopulatedXmlError()
        {
            return new SignalMessage
            {
                MessageInfo = new MessageInfo {MessageId = Guid.NewGuid().ToString(), Timestamp = DateTime.UtcNow},
                Error =
                    new[]
                    {
                        new Error
                        {
                            category = "myCategory",
                            Description = new Description {lang = "en", Value = "this is a long description"},
                            errorCode = "errorCode",
                            ErrorDetail = "errorDetail",
                            origin = "origin",
                            refToMessageInError = "refToMessageInError",
                            severity = "severity",
                            shortDescription = "shortDescription"
                        }
                    }
            };
        }
    }
}