using System;
using Eu.EDelivery.AS4.Mappings.Core;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Xml;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Core
{
    /// <summary>
    /// Testing <see cref="SignalMessageMap"/>
    /// </summary>
    public class GivenXmlSignalMessageMapFacts
    {
        public class GivenValidXmlReceiptSignalMessage : GivenXmlSignalMessageMapFacts
        {
            [Fact]
            public void ThenMapToReceiptSucceeds()
            {
                SignalMessage receipt = GetReceiptXmlSignalMessage();

                var result = AS4Mapper.Map<AS4.Model.Core.Receipt>(receipt);

                Assert.Equal(receipt.MessageInfo.MessageId, result.MessageId);
                Assert.Equal(receipt.MessageInfo.RefToMessageId, result.RefToMessageId);
                Assert.Equal(receipt.MessageInfo.Timestamp, result.Timestamp);
            }
            
            private static SignalMessage GetReceiptXmlSignalMessage()
            {
                return new SignalMessage
                {
                    Receipt = new Receipt(),
                    MessageInfo =
                        new MessageInfo
                        {
                            MessageId = "abc",
                            RefToMessageId = Guid.NewGuid().ToString(),
                            Timestamp = new DateTime(2016, 12, 14)
                        }
                };
            }
        }
    }
}