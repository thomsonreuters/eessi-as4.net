using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps
{
    public class DummyStep : IStep
    {
        /// <summary>
        /// Execute the step for a given <paramref name="internalMessage"/>.
        /// </summary>
        /// <param name="internalMessage">Message used during the step execution.</param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(MessagingContext internalMessage)
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task FailsToExecuteStep()
        {
            await Assert.ThrowsAnyAsync<Exception>(
                () => new DummyStep().ExecuteAsync(internalMessage: null));
        }
    }
}