using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Exceptions.Handlers;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Submit;
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

            using (var bodyStore = new InMemoryMessageBodyStore())
            using (var stream = new MemoryStream(expectedBody))
            {
                var sut = new PullSendAgentExceptionHandler(GetDataStoreContext, StubConfig.Default, bodyStore);

                // Act
                await sut.HandleTransformationException(
                    new Exception(_expectedMessage),
                    new ReceivedMessage(stream));
            }
            // Assert
            GetDataStoreContext.AssertOutException(
                ex =>
                {
                    Assert.NotNull(ex);
                    Assert.Equal(_expectedMessage, ex.Exception);
                });
        }

        [Fact]
        public async Task InsertOutException_IfHandlingExecutionException_DuringReceive()
        {
            await TestExecutionException(
                async sut =>
                    await sut.HandleExecutionException(
                        new Exception(_expectedMessage),
                        new MessagingContext(AS4Message.Empty, MessagingContextMode.Receive)),
                assertLocation: Assert.Null);
        }

        [Fact]
        public async Task InsertOutExcception_IfHandlingExcutionException_DuringSubmit()
        {
            await TestExecutionException(
                async sut =>
                    await sut.HandleExecutionException(
                        new Exception(_expectedMessage),
                        new MessagingContext(new SubmitMessage())),
                assertLocation: Assert.NotNull);
        }

        [Fact]
        public async Task InsertOutException_IfHandlingErrorException_DuringReceive()
        {
            await TestExecutionException(
                async sut =>
                    await sut.HandleErrorException(
                        new Exception(_expectedMessage),
                        new MessagingContext(AS4Message.Empty, MessagingContextMode.Receive)),
                assertLocation: Assert.Null);
        }

        [Fact]
        public async Task InsertOutException_IfHandlingErrorException_DuringSubmit()
        {
            await TestExecutionException(
                async sut =>
                    await sut.HandleErrorException(
                        new Exception(_expectedMessage),
                        new MessagingContext(new SubmitMessage())),
                assertLocation: Assert.NotNull);
        }

        private async Task TestExecutionException(
            Func<IAgentExceptionHandler, Task<MessagingContext>> act,
            Action<string> assertLocation)
        {
            // Arrange
            var sut = new PullSendAgentExceptionHandler(GetDataStoreContext, StubConfig.Default, new InMemoryMessageBodyStore());

            // Act
            await act(sut);

            // Assert            
            GetDataStoreContext.AssertOutException(
                ex =>
                {
                    Assert.True(ex.Exception.IndexOf(_expectedMessage, StringComparison.CurrentCultureIgnoreCase) > -1);
                    assertLocation(ex.MessageLocation);
                });
        }
    }
}
