using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Xunit;
using AgreementReference = Eu.EDelivery.AS4.Model.PMode.AgreementReference;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.PMode
{
    /// <summary>
    /// Testing <see cref="PModeAgreementRefResolver" />
    /// </summary>
    public class GivenPModeAgreemenRefResolverFacts
    {
        public class GivenValidArguments : GivenPModeAgreemenRefResolverFacts
        {
            private static AgreementReference CreateDefaultAgreementRef()
            {
                return new AgreementReference {Value = "name", Type = "type", PModeId = "pmode-id"};
            }

            [Fact]
            public void ThenResolverGetsAgreementRef()
            {
                // Arrange
                SendingProcessingMode pmode = CreateSendingPMode(includePMode: false);
                var resolver = new PModeAgreementRefResolver();

                // Act
                AS4.Model.Core.AgreementReference agreementRef = resolver.Resolve(pmode).UnsafeGet;

                // Assert
                AgreementReference pmodeRef = pmode.MessagePackaging.CollaborationInfo.AgreementReference;
                Assert.Equal(pmodeRef.Value, agreementRef.Value);
                Assert.Equal(Maybe.Just(pmodeRef.Type), agreementRef.Type);
                Assert.NotEqual(Maybe.Just(pmode.Id), agreementRef.PModeId);
            }

            [Fact]
            public void ThenResolverGetsAgreementRefWithPModeId()
            {
                // Arrange
                SendingProcessingMode pmode = CreateSendingPMode(includePMode: true);
                var resolver = new PModeAgreementRefResolver();

                // Act
                AS4.Model.Core.AgreementReference agreementRef = resolver.Resolve(pmode).UnsafeGet;

                // Assert
                AgreementReference pmodeRef = pmode.MessagePackaging.CollaborationInfo.AgreementReference;
                Assert.Equal(pmodeRef.Value, agreementRef.Value);
                Assert.Equal(Maybe.Just(pmodeRef.Type), agreementRef.Type);
                Assert.Equal(Maybe.Just(pmode.Id), agreementRef.PModeId);
            }

            private static SendingProcessingMode CreateSendingPMode(bool includePMode)
            {
                return new SendingProcessingMode
                {
                    Id = "pmode-id",
                    MessagePackaging =
                    {
                        IncludePModeId = includePMode,
                        CollaborationInfo = new AS4.Model.PMode.CollaborationInfo {AgreementReference = CreateDefaultAgreementRef()}
                    }
                };
            }
        }
    }
}