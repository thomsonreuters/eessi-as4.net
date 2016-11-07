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
        private readonly IDictionary<string, string> _dictionary;
        private readonly string _key = "$mandatory-key$";
        private readonly string _value = "$mandatory-value$";

        public GivenIDictionaryExtensionFacts()
        {
            this._dictionary = new Dictionary<string, string> {[this._key] = this._value};
        }

        /// <summary>
        /// Testing if the IDictionaryExtensions succeeds
        /// </summary>
        public class GivenIDictionaryExtesionSucceeds
            : GivenIDictionaryExtensionFacts
        {
            [Fact]
            public void ThenDemoteSucceeds()
            {
                // Act
                this._dictionary.Demote(this._key);
                // Assert
                Assert.DoesNotContain(
                    this._dictionary.Keys,
                    k => k.Equals(this._key));
            }

            [Fact]
            public void ThenFlattenSucceeds()
            {
                // Act
                string result = this._dictionary.Flatten();
                // Assert
                Assert.Equal($"{this._key}:{this._value}", result);
            }

            [Fact]
            public void ThenMergeSucceeds()
            {
                // Arrange
                string extraValue = "$extra-value$";
                string newKey = "$new-key$";
                // Act
                this._dictionary.Merge(this._key, extraValue);
                this._dictionary.Merge(newKey, extraValue);
                // Assert
                Assert.Equal(this._dictionary[this._key], extraValue);
                Assert.Equal(this._dictionary[newKey], extraValue);
            }

            [Fact]
            public void ThenReadMandatoryPropertySucceeds()
            {
                // Act
                string value = this._dictionary.ReadMandatoryProperty(this._key);
                // Assert
                Assert.Same(this._value, value);
            }

            [Fact]
            public void ThenReadOptionalPropertySucceeds()
            {
                // Arrange
                string defaultValue = "$default$";
                // Act
                string value = this._dictionary.ReadOptionalProperty("doesn't exist", defaultValue);
                // Assert
                Assert.Same(defaultValue, value);
            }
        }

        /// <summary>
        /// Testing if the IDictionaryExtensions fails
        /// </summary>
        public class GivenIDictionaryExtensionsFails
            : GivenIDictionaryExtensionFacts
        {
            [Fact]
            public void ThenReadMandatoryPropertyFails()
            {
                // Arrange
                string doesntExistedKey = "$doesn't existed key$";
                // Act / Assert
                Assert.Throws<AS4Exception>(
                    () => this._dictionary.ReadMandatoryProperty(doesntExistedKey));
            }
        }
    }
}