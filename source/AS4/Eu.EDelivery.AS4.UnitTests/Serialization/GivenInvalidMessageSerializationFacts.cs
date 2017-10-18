using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Serialization;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Serialization
{
    public class GivenInvalidMessageSerializationFacts
    {
        [Fact]
        public async Task CanDeserializeReceiptWithInvalidNRI()
        {
            const string contentType = @"multipart/related;boundary=""NSMIMEBoundary__5c65cf85-89d2-4921-9029-63df49ef2fa2"";type=""application/soap+xml"";start=""<_7a711d7c-4d1c-4ce7-ab38-794a01b445e1>""";

            using (var receiptStream = GetStreamWithInvalidReceipt())
            {
                var serializer = SerializerProvider.Default.Get(contentType);

                var receipt = await serializer.DeserializeAsync(receiptStream, contentType, CancellationToken.None);

                Assert.NotNull(receipt);
            }
        }

        private static Stream GetStreamWithInvalidReceipt()
        {
            return new MemoryStream(Properties.Resources.Receipt_InvalidNRI_Namespace);
        }
    }
}
