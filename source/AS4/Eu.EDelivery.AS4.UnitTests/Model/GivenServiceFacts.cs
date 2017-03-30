using Eu.EDelivery.AS4.Model.Core;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    /// <summary>
    /// Testing <see cref="Service" />
    /// </summary>
    public class GivenServiceFacts
    {
        private readonly Service _service;

        public GivenServiceFacts()
        {
            _service = new Service();
        }

        /// <summary>
        /// Testing if the Service succeeds
        /// </summary>
        public class GivenServiceSucceeds : GivenServiceFacts
        {
            [Fact]
            public void ThenGetHashCodeSucceeds()
            {
                // Arrange
                _service.Value = "Service Name";
                _service.Type = "Service Type";

                // Act
                int hashCode = _service.GetHashCode();

                // Assert
                Assert.False(hashCode == 0);
            }

            [Fact]
            public void ThenTwoServicesAreEqualSucceeds()
            {
                // Arrange
                var serviceA = new Service();
                Service serviceB = serviceA;

                serviceA.Value = serviceB.Value = "Service Name";
                serviceA.Type = serviceB.Type = "Service Type";

                // Act
                bool isEqual = serviceA.Equals(serviceB);

                // Assert
                Assert.True(isEqual);
            }
        }

        /// <summary>
        /// Testing if the Service fails
        /// </summary>
        public class GivenServiceFails : GivenServiceFacts
        {
            [Fact]
            public void ThenTwoServicesAreEqualFails()
            {
                // Arrange
                var serviceA = new Service {Value = "Service Name", Type = "Service Type"};

                // Act
                bool isEqual = serviceA.Equals(null);

                // Assert
                Assert.False(isEqual);
            }
        }
    }
}