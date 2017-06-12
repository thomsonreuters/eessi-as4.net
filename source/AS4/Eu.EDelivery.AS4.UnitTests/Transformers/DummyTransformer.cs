using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Transformers;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    public class DummyTransformer : ITransformer
    {
        /// <summary>
        /// Transform a given <see cref="ReceivedMessage"/> to a Canonical <see cref="InternalMessage"/> instance.
        /// </summary>
        /// <param name="message">Given message to transform.</param>
        /// <param name="cancellationToken">Cancellation which stops the transforming.</param>
        /// <returns></returns>
        public Task<MessagingContext> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task FailsToTransform()
        {
            await Assert.ThrowsAnyAsync<Exception>(
                () => new DummyTransformer().TransformAsync(message: null, cancellationToken: CancellationToken.None));
        }
    }
}