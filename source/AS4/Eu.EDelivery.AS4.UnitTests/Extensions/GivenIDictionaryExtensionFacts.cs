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
        private readonly IDictionary<string, string> _dictionary;
        private readonly string _key = "$mandatory-key$";
        private readonly string _value = "$mandatory-value$";

        public GivenIDictionaryExtensionFacts()
        {
            _dictionary = new Dictionary<string, string> {[_key] = _value};
        }

        /// <summary>
        /// Testing if the IDictionaryExtensions succeeds
        /// </summary>
        public class GivenIDictionaryExtesionSucceeds : GivenIDictionaryExtensionFacts
        {
            [Fact]
            public void ThenDemoteSucceeds()
            {
                // Act
                _dictionary.Demote(_key);

                // Assert
                Assert.DoesNotContain(_dictionary.Keys, k => k.Equals(_key));
            }

            [Fact]
            public void ThenFlattenSucceeds()
            {
                // Act
                string result = _dictionary.Flatten();

                // Assert
                Assert.Equal($"{_key}:{_value}", result);
            }

            [Fact]
            public void ThenMergeSucceeds()
            {
                // Arrange
                const string extraValue = "$extra-value$";
                const string newKey = "$new-key$";

                // Act
                _dictionary.Merge(_key, extraValue);
                _dictionary.Merge(newKey, extraValue);

                // Assert
                Assert.Equal(_dictionary[_key], extraValue);
                Assert.Equal(_dictionary[newKey], extraValue);
            }

            [Fact]
            public void ThenReadMandatoryPropertySucceeds()
            {
                // Act
                string value = _dictionary.ReadMandatoryProperty(_key);

                // Assert
                Assert.Same(_value, value);
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
                Assert.Throws<AS4Exception>(() => _dictionary.ReadMandatoryProperty(doesntExistedKey));
            }
        }
    }
}