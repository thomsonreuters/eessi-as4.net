using System;
using Eu.EDelivery.AS4.Receivers.Specifications;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Receivers.Specifications
{
    /// <summary>
    /// Testing <see cref="DatastoreSpecificationArgs"/>
    /// </summary>
    public class GivenDatastoreSpecificationArgsFacts
    {
        [Theory]
        [InlineData("InMessages", null)]
        [InlineData("", "Operation = ToBeSent")]
        public void FailsToConstruct_IfRequiredArgumentIsMissing(string tableName, string filter)
        {
            // Act / Assert
            Assert.Throws<ArgumentException>(() => new DatastoreSpecificationArgs(tableName, filter));
        }
    }
}
