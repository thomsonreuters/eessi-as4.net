using System;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Receivers.Utilities;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Receivers.Specifications
{
    /// <summary>
    /// Testing <see cref="Conversion"/>
    /// </summary>
    public class GivenConversionFacts
    {
        [Theory]
        [InlineData("ToBeSent", typeof(Operation))]
        [InlineData("1", typeof(int))]
        public void ConvertExpected(string value, Type expectedType)
        {            
            // Act
            object actual = Conversion.Convert(expectedType, value);

            // Assert
            Assert.IsType(expectedType, actual);
        }
    }
}
