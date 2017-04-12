using Eu.EDelivery.AS4.Mappings.Core;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Xml;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Core
{
    /// <summary>
    /// Testing <see cref="AgreementRefMap"/>
    /// </summary>
    public class GivenAgreementRefMapFacts
    {
        [Fact]
        public void SucceedsFromXmlToCore()
        {
            // Arrange
            var expcectedReference = new AgreementRef {Value = "http://agreements.holodeckb2b.org/examples/agreement1"};

            // Act
            var actualReference = AS4Mapper.Map<AgreementReference>(expcectedReference);

            // Assert
            Assert.Equal(expcectedReference.Value, actualReference.Value);
        }
    }
}
