using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions.Handlers;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Exceptions.Handlers
{
    public class GivenOutExceptionHandlerFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task InsertOutException_IfTransformException()
        {
            // Arrange
            var sut = new OutExceptionHandler(GetDataStoreContext);
            string expected = Guid.NewGuid().ToString();

            // Act
            await sut.HandleTransformationException(new Exception(expected));

            // Assert
            GetDataStoreContext.AssertOutException(ex => Assert.Equal(expected, ex.Exception));
        }
    }
}
