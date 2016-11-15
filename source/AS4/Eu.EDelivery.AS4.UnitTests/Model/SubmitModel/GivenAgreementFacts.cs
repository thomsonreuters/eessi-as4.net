using Eu.EDelivery.AS4.Model.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Model.SubmitModel
{
    /// <summary>
    /// Testing <see cref="Agreement"/>
    /// </summary>
    public class GivenAgreementFacts
    {
        protected Agreement CreateAgreement(string value, string type, string pmodeId)
        {
            return new Agreement
            {
                Value = value,
                RefType = type,
                PModeId = pmodeId
            };
        }

        public class GivenValidArguments : GivenAgreementFacts
        {
            [Theory, InlineData("shared-value", "shared-type", "shared-pmode-id")]
            public void ThenTwoAgreementsAreEqual(string sharedValue, string sharedType, string sharedPModeId)
            {
                // Arrange
                Agreement agreementA = base.CreateAgreement(sharedValue, sharedType, sharedPModeId);
                Agreement agreementB = agreementA;
                // Act
                bool isEqual = agreementA.Equals(agreementB);
                // Assert
                Assert.True(isEqual);
            }

            [Theory, InlineData("shared-value", "shared-type", "shared-pmode-id")]
            public void ThenTwoAgreementsAreEqualForProperties(string sharedValue, string sharedType, string sharedPModeId)
            {
                // Arrange
                Agreement agreementA = base.CreateAgreement(sharedValue, sharedType, sharedPModeId);
                Agreement agreementB = base.CreateAgreement(sharedValue, sharedType, sharedPModeId);
                // Act
                bool isEqual = agreementA.Equals(agreementB);
                // Assert
                Assert.True(isEqual);
            }

            [Theory, InlineData("shared-value", "shared-type", "shared-pmode-id")]
            public void ThenTwoAgreementsAreEqualForObject(string sharedValue, string sharedType, string sharedPModeId)
            {
                // Arrange
                Agreement agreementA = base.CreateAgreement(sharedValue, sharedType, sharedPModeId);
                Agreement agreementB = base.CreateAgreement(sharedValue, sharedType, sharedPModeId);
                // Act
                bool isEqual = agreementA.Equals((object)agreementB);
                // Assert
                Assert.True(isEqual);
            }

            [Theory, InlineData("shared-value", "shared-type", "shared-pmode-id")]
            public void ThenTwoAgreementsAreNotEqualForValue(string sharedValue, string sharedType, string sharedPModeId)
            {
                // Arrange
                Agreement agreementA = base.CreateAgreement(sharedValue, sharedType, sharedPModeId);
                Agreement agreementB = base.CreateAgreement("not-equal", sharedType, sharedPModeId);
                // Act
                bool isEqual = agreementA.Equals(agreementB);
                // Assert
                Assert.False(isEqual);
            }

            [Theory, InlineData("shared-value", "shared-type", "shared-pmode-id")]
            public void ThenTwoAgreementsAreNotEqualForType(string sharedValue, string sharedType, string sharedPModeId)
            {
                // Arrange
                Agreement agreementA = base.CreateAgreement(sharedValue, sharedType, sharedPModeId);
                Agreement agreementB = base.CreateAgreement(sharedValue, "not-equal", sharedPModeId);
                // Act
                bool isEqual = agreementA.Equals(agreementB);
                // Assert
                Assert.False(isEqual);
            }

            [Theory, InlineData("shared-value", "shared-type", "shared-pmode-id")]
            public void ThenTwoAgreementsAreNotEqualForPModeid(string sharedValue, string sharedType, string sharedPModeId)
            {
                // Arrange
                Agreement agreementA = base.CreateAgreement(sharedValue, sharedType, sharedPModeId);
                Agreement agreementB = base.CreateAgreement(sharedValue, sharedType, "not-equal");
                // Act
                bool isEqual = agreementA.Equals(agreementB);
                // Assert
                Assert.False(isEqual);
            }
        }

        public class GivenInvalidAgruments : GivenAgreementFacts
        {
            [Theory, InlineData("shared-value", "shared-type", "shared-pmode-id")]
            public void ThenTwoAgreementsAreNotEqualForNull(string sharedValue, string sharedType, string sharedPModeId)
            {
                // Arrange
                Agreement agreementA = base.CreateAgreement(sharedValue, sharedType, sharedPModeId);
                Agreement agreementB = null;
                // Act
                bool isEqual = agreementA.Equals(agreementB);
                // Assert
                Assert.False(isEqual);
            }
        }
    }
}
