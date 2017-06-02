using Eu.EDelivery.AS4.Model.Submit;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Submit
{
    /// <summary>
    /// Testing <see cref="SubmitMessage"/>
    /// </summary>
    public class GivenSubmitMessageFacts
    {
        [Theory]
        [InlineData("", true)]
        [InlineData("not empty", false)]
        public void IsEmpty_IfPModeIdIsEmpty(string pmodeId, bool expected)
        {
            // Arrange
            var sut = new SubmitMessage {Collaboration = {AgreementRef = {PModeId = pmodeId}}};
            
            // Act / Assert
            Assert.Equal(expected, sut.IsEmpty);
        }
    }
}
