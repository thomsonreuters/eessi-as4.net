using Eu.EDelivery.AS4.Entities;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Entities
{
    /// <summary>
    /// Testing <see cref="Entity"/>
    /// </summary>
    public class GivenEntityFacts
    {
        [Fact]
        public void IsTransient()
        {
            // Arrange
            var entity = new Entity();
            
            // Act
            entity.InitializeIdFromDatabase(1);

            // Assert
            Assert.False(entity.IsTransient);
            Assert.True(new Entity().IsTransient);
        }
    }
}
