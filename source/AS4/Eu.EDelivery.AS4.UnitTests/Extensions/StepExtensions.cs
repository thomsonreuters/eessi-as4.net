using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;

namespace Eu.EDelivery.AS4.UnitTests.Extensions
{
    /// <summary>
    /// Extensions on the <see cref="IStep"/> interface for better readability across the tests.
    /// </summary>
    public static class StepExtensions
    {
        /// <summary>
        /// Runs the <see cref="IStep.ExecuteAsync"/> method on the given <see cref="IStep"/> with no cancellation.
        /// </summary>
        /// <param name="sut">The sut.</param>
        /// <param name="ctx">The CTX.</param>
        /// <returns></returns>
        public static async Task<StepResult> ExecuteAsync(this IStep sut, MessagingContext ctx)
        {
            return await sut.ExecuteAsync(ctx, CancellationToken.None);
        }
    }
}
