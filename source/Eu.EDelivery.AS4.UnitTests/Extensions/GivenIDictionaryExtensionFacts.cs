using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Extensions;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Extensions
{
    /// <summary>
    /// Testing <see cref="AS4.Extensions.DictionaryExtensions" />
    /// </summary>
    public class GivenIDictionaryExtensionFacts
    {
        private const string TestKey = "$mandatory-key$";
        private const string TestValue = "$mandatory-value$";
        private readonly IDictionary<string, string> _dictionary;

        public GivenIDictionaryExtensionFacts()
        {
            _dictionary = new Dictionary<string, string> {[TestKey] = TestValue};
        }

        /// <summary>
        /// Testing if the IDictionaryExtensions succeeds
        /// </summary>
        public class GivenIDictionaryExtesionSucceeds : GivenIDictionaryExtensionFacts
        {
            [Fact]
            public void ThenReadMandatoryPropertySucceeds()
            {
                // Act
                string value = _dictionary.ReadMandatoryProperty(TestKey);

                // Assert
                Assert.Same(TestValue, value);
            }

            [Fact]
            public void ThenReadOptionalPropertySucceeds()
            {
                // Arrange
                const string defaultValue = "$default$";

                // Act
                string value = _dictionary.ReadOptionalProperty("doesn't exist", defaultValue);

                // Assert
                Assert.Same(defaultValue, value);
            }
        }

        /// <summary>
        /// Testing if the IDictionaryExtensions fails
        /// </summary>
        public class GivenIDictionaryExtensionsFails : GivenIDictionaryExtensionFacts
        {
            [Fact]
            public void ThenReadMandatoryPropertyFails()
            {
                // Arrange
                const string doesntExistedKey = "$doesn't existed key$";

                // Act / Assert
                Assert.ThrowsAny<Exception>(() => _dictionary.ReadMandatoryProperty(doesntExistedKey));
            }
        }
    }
}