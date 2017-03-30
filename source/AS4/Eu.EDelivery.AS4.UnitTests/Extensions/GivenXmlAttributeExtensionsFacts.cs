using System;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.UnitTests.Receivers;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Extensions
{
    /// <summary>
    /// Testing <see cref="XmlAttributeExtensions"/>
    /// </summary>
    public class GivenXmlAttributeExtensionsFacts
    {
        public class AsTimeSpan
        {
            [Fact]
            public void SucceedsWithValidTimeSpanValue()
            {
                // Arrange
                var xmlAttribute = new StubXmlAttribute("tmin", "0:00:01");
                
                // Act
                TimeSpan actualTimeSpan = xmlAttribute.AsTimeSpan();

                // Assert
                Assert.Equal(TimeSpan.FromSeconds(1), actualTimeSpan);
            }

            [Fact]
            public void FailsWithInvalidTimeSpanValue()
            {
                // Arrange
                var xmlAttribute = new StubXmlAttribute("tmax", "unknown-value");
                
                // Act
                TimeSpan actualTimeSpan = xmlAttribute.AsTimeSpan();

                // Assert
                Assert.Equal(default(TimeSpan), actualTimeSpan);
            }

            [Fact]
            public void FailsWithNullTimeSpanValue()
            {
                // Arrange
                var setting = new Setting("key", "value");

                // Act
                TimeSpan actualTimeSpan = setting["unknown-attribute"].AsTimeSpan();
                
                // Assert
                Assert.Equal(default(TimeSpan), actualTimeSpan);
            }
        }
    }
}
