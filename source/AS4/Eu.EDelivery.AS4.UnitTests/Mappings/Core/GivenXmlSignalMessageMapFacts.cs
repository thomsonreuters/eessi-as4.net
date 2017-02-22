using System;
using AutoMapper;
using Eu.EDelivery.AS4.Mappings.Common;
using Eu.EDelivery.AS4.Xml;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Core
{
    public class GivenXmlSignalMessageMapFacts
    {
        
        public class GivenValidXmlReceiptSignalMessage : GivenXmlSignalMessageMapFacts
        {
            [Fact]
            public void ThenMapToReceiptSucceeds()
            {
                var receipt = GetReceiptXmlSignalMessage();

                var result = Mapper.Map<Eu.EDelivery.AS4.Model.Core.Receipt>(receipt);

                Assert.Equal(receipt.MessageInfo.MessageId, result.MessageId);
                Assert.Equal(receipt.MessageInfo.RefToMessageId, result.RefToMessageId);
                Assert.Equal(receipt.MessageInfo.Timestamp, result.Timestamp);
            }
        }


        private static Xml.SignalMessage GetReceiptXmlSignalMessage()
        {
            var result = new Xml.SignalMessage();
            result.Receipt = new Receipt();
            result.MessageInfo = new MessageInfo();
            result.MessageInfo.MessageId = "abc";
            result.MessageInfo.RefToMessageId = Guid.NewGuid().ToString();
            result.MessageInfo.Timestamp = new DateTime(2016, 12, 14);

            return result;
        }

    }
}
