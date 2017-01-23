using Eu.EDelivery.AS4.Serialization;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Serialization
{
    public class GivenSerializerProviderFacts
    {
        protected ISerializerProvider Provider;

        public GivenSerializerProviderFacts()
        {
            Provider = new SerializerProvider();
        }

        public class GivenValidArguments : GivenSerializerProviderFacts
        {
            [Theory]
            [InlineData(Constants.ContentTypes.Mime)]
            [InlineData(Constants.ContentTypes.Soap)]
            public void ThenCanProvideSerializer(string contentType)
            {
                var serializer = Provider.Get(contentType);

                Assert.NotNull(serializer);

                if (contentType == Constants.ContentTypes.Soap)
                {
                    Assert.IsType<SoapEnvelopeSerializer>(serializer);
                }
                if (contentType == Constants.ContentTypes.Mime)
                {
                    Assert.IsType<MimeMessageSerializer>(serializer);
                }
            }
        }
    }
}
