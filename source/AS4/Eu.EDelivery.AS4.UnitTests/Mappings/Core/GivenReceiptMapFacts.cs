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

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Core
{
    /// <summary>
    /// Testing <see cref="ReceiptMap"/>
    /// </summary>
    public class GivenReceiptMapFacts
    {
        public GivenReceiptMapFacts()
        {
            MapInitialization.InitializeMapper();
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        public class GivenValidArguments : GivenReceiptMapFacts
        {
            [Fact]
            public async void ThenMapModelToXmlReceiptSucceeds()
            {
                // Arrange
                AS4Message as4Message = await base.GetPopulatedModelReceipt();
                var coreReceipt = as4Message.PrimarySignalMessage as CoreReceipt;
                // Act
                var xmlReceipt = Mapper.Map<XmlReceipt>(coreReceipt);
                // Assert
                AssertReceiptReferences(coreReceipt, xmlReceipt);
            }

            private void AssertReceiptReferences(CoreReceipt coreReceipt, XmlReceipt xmlReceipt)
            {
                XmlInformation[] xmlInformations = xmlReceipt.NonRepudiationInformation.MessagePartNRInformation;
                ICollection<CoreInformation> coreInformations = coreReceipt.NonRepudiationInformation.MessagePartNRInformation;

                foreach (XmlInformation nrInformation in xmlInformations)
                {
                    var xmlReference = nrInformation.Item as Xml.ReferenceType;
                    Reference coreReference = coreInformations
                        .FirstOrDefault(i => i.Reference.URI.Equals(xmlReference.URI)).Reference;

                    AssertReceiptReference(xmlReference, coreReference);
                }
            }

            private void AssertReceiptReference(ReferenceType xmlReference, Reference coreReference)
            {
                AssertDigestValue(xmlReference.DigestValue, coreReference.DigestValue);
                AssertDigestMethod(xmlReference.DigestMethod.Algorithm, coreReference.DigestMethod.Algorithm);
            }

            private void AssertDigestValue(byte[] xmlDigestValue, string coreDigestValue)
            {
                string xmlDigestValueString = Encoding.UTF8.GetString(xmlDigestValue);
                Assert.Equal(xmlDigestValueString, coreDigestValue);
            }

            private void AssertDigestMethod(string xmlDigestMethod, string coreDigestMethod)
            {
                Assert.Equal(xmlDigestMethod, coreDigestMethod);
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