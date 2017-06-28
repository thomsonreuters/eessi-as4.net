using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Exceptions.Handlers;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Exceptions.Handlers
{
    public class GivenPullSendAgentExceptionHandlerFacts : GivenDatastoreFacts
    {
        private readonly string _expectedMessage = Guid.NewGuid().ToString();

        [Fact]
        public async Task InsertInException_IfHandlingTransformException()
        {
            // Arrange
            byte[] expectedBody = Encoding.UTF8.GetBytes("serialize me!");
            var sut = new PullSendAgentExceptionHandler(GetDataStoreContext);

            // Act
            await sut.HandleTransformationException(
                new Exception(_expectedMessage),
                new MemoryStream(expectedBody));

            // Assert
            GetDataStoreContext.AssertOutException(
                ex =>
                {
                    Assert.Equal(_expectedMessage, ex.Exception);
                    Assert.Equal(expectedBody, ex.MessageBody);
                });
        }

        [Fact]
        public async Task InsertInException_IfHandlingExecutionException()
        {
            await TestExecutionException(
                async sut =>
                    await sut.HandleExecutionException(
                        new Exception(_expectedMessage),
                        new MessagingContext(AS4Message.Empty, MessagingContextMode.Receive)));
        }

        [Fact]
        public async Task InsertInException_IfHandlingErrorException()
        {
            await TestExecutionException(
                async sut =>
                    await sut.HandleErrorException(
                        new Exception(_expectedMessage),
                        new MessagingContext(AS4Message.Empty, MessagingContextMode.Receive)));
        }

        private async Task TestExecutionException(Func<IAgentExceptionHandler, Task<MessagingContext>> act)
        {
            // Arrange
            var sut = new PullSendAgentExceptionHandler(GetDataStoreContext);

            // Act
            MessagingContext result = await act(sut);

            // Assert
            Assert.IsType<Error>(result.AS4Message.PrimarySignalMessage);
            GetDataStoreContext.AssertOutException(
                ex =>
                {
                    Assert.Equal(_expectedMessage, ex.Exception);
                    Assert.NotNull(ex.MessageBody);
                });
        }
    }
}
