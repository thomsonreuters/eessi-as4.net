using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Mappings.Common;
using Eu.EDelivery.AS4.Mappings.Core;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.Xml;
using Xunit;
using XmlReceipt = Eu.EDelivery.AS4.Xml.Receipt;
using CoreReceipt = Eu.EDelivery.AS4.Model.Core.Receipt;
using XmlInformation = Eu.EDelivery.AS4.Xml.MessagePartNRInformation;
using CoreInformation = Eu.EDelivery.AS4.Model.Core.MessagePartNRInformation;
using System;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Core
{
    /// <summary>
    /// Testing <see cref="ErrorMap"/>
    /// </summary>
    public class GivenErrorMapFacts
    {
        public GivenErrorMapFacts()
        {
            MapInitialization.InitializeMapper();
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        public class GivenValidArguments : GivenErrorMapFacts
        {
            [Fact]
            public void ThenMessageIdIsCorrectlyMapped()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                var signalMessage = new Eu.EDelivery.AS4.Xml.SignalMessage()
                {
                    MessageInfo = new Eu.EDelivery.AS4.Xml.MessageInfo()
                    {
                        MessageId = messageId,
                        Timestamp = DateTime.UtcNow
                    },
                    Error = new[] { new Eu.EDelivery.AS4.Xml.Error() }
                };

                // Act
                var error = Mapper.Map<AS4.Model.Core.Error>(signalMessage);

                // Assert
                Assert.Equal(messageId, error.MessageId);
            }

        }

        protected async Task<AS4Message> GetPopulatedModelReceipt()
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(Properties.Resources.receipt));
            var serializer = new SoapEnvelopeSerializer();

            return await serializer.DeserializeAsync(
                memoryStream, Constants.ContentTypes.Soap, CancellationToken.None);
        }
    }
}