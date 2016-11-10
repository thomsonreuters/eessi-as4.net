using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.PMode
{
    /// <summary>
    /// Testing <see cref="PModeAgreementRefResolver"/>
    /// </summary>
    public class GivenPModeAgreemenRefResolverFacts
    {
        public class GivenValidArguments : GivenPModeAgreemenRefResolverFacts
        {
            [Fact]
            public void ThenResolverGetsAgreementRef()
            {
                // Arrange
                var pmode = new SendingProcessingMode();
                pmode.Id = "pmode-id";
                pmode.MessagePackaging.IncludePModeId = false;
                pmode.MessagePackaging.CollaborationInfo = new CollaborationInfo();
                pmode.MessagePackaging.CollaborationInfo.AgreementReference = CreateDefaultAgreementRef();
                var resolver = new PModeAgreementRefResolver();
                // Act
                AgreementReference agreementRef = resolver.Resolve(pmode);
                // Assert
                AgreementReference pmodeRef = pmode.MessagePackaging.CollaborationInfo.AgreementReference;
                Assert.Equal(pmodeRef.Name, agreementRef.Name);
                Assert.Equal(pmodeRef.Type, agreementRef.Type);
                Assert.NotEqual(pmode.Id, agreementRef.PModeId);
            }

            [Fact]
            public void ThenResolverGetsAgreementRefWithPModeId()
            {
                // Arrange
                var pmode = new SendingProcessingMode();
                pmode.Id = "pmode-id";
                pmode.MessagePackaging.IncludePModeId = true;
                pmode.MessagePackaging.CollaborationInfo = new CollaborationInfo();
                pmode.MessagePackaging.CollaborationInfo.AgreementReference = CreateDefaultAgreementRef();
                var resolver = new PModeAgreementRefResolver();
                // Act
                AgreementReference agreementRef = resolver.Resolve(pmode);
                // Assert
                AgreementReference pmodeRef = pmode.MessagePackaging.CollaborationInfo.AgreementReference;
                Assert.Equal(pmodeRef.Name, agreementRef.Name);
                Assert.Equal(pmodeRef.Type, agreementRef.Type);
                Assert.Equal(pmode.Id, agreementRef.PModeId);
            }

            private AgreementReference CreateDefaultAgreementRef()
            {
                return new AgreementReference { Name = "name", Type = "type" };
            }
        }
    }
}
