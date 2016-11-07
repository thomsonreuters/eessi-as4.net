using Eu.EDelivery.AS4.Model;
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
            this._service = new Service();
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
                this._service.Name = "Service Name";
                this._service.Type = "Service Type";
                // Act
                int hashCode = this._service.GetHashCode();
                // Assert
                Assert.False(hashCode == 0);
            }

            [Fact]
            public void ThenTwoServicesAreEqualSucceeds()
            {
                // Arrange
                var serviceA = new Service();
                Service serviceB = serviceA;

                serviceA.Name = serviceB.Name = "Service Name";
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
                var serviceA = new Service
                {
                    Name = "Service Name",
                    Type = "Service Type"
                };

                // Act
                bool isEqual = serviceA.Equals(null);

                // Assert
                Assert.False(isEqual);
            }
        }
    }
}