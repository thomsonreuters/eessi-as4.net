using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Submit;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Submit
{
    /// <summary>
    /// Testing <see cref="StoreAS4MessageStep" />
    /// </summary>
    public class GivenStoreAS4MessageStepsFacts : GivenDatastoreFacts
    {
        private readonly StoreAS4MessageStep _module;

        public GivenStoreAS4MessageStepsFacts()
        {
            _module = new StoreAS4MessageStep();
        }

        /// <summary>
        /// Testing if the module succeeds
        /// </summary>
        public class GivenStoreAs4MessageStepsSucceeds : GivenStoreAS4MessageStepsFacts
        {
            [Fact]
            public async Task ThenTransmitMessageSucceedsAsync()
            {
                // Arrange
                AS4Message message = new AS4MessageBuilder().Build();
                var internalMessage = new InternalMessage(message);

                // Act
                StepResult result = await _module.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
            }
        }

        /// <summary>
        /// Testing if the module fails
        /// </summary>
        public class GivenStoreAs4MessageStepsFails : GivenStoreAS4MessageStepsFacts
        {
            [Fact]
            public async Task ThenTransmitMessageFailsWithNullAS4MessageAsync()
            {
                // Act / Assert
                await Assert.ThrowsAsync<NullReferenceException>(
                    () => _module.ExecuteAsync(null, CancellationToken.None));
            }
        }
    }
}