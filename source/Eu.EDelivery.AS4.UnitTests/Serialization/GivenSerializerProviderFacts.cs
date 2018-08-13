using System;
using Eu.EDelivery.AS4.Serialization;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Serialization
{
    public class GivenSerializerProviderFacts
    {
        private readonly ISerializerProvider _provider;

        public GivenSerializerProviderFacts()
        {
            _provider = new SerializerProvider();
        }

        public class GivenValidArguments : GivenSerializerProviderFacts
        {
            [Theory]
            [InlineData(Constants.ContentTypes.Mime, typeof(MimeMessageSerializer))]
            [InlineData(Constants.ContentTypes.Soap, typeof(SoapEnvelopeSerializer))]
            public void ThenCanProvideSerializer(string contentType, Type expectedType)
            {
                ISerializer serializer = _provider.Get(contentType);

                Assert.NotNull(serializer);
                Assert.IsType(expectedType, serializer);
            }
        }
    }
}
