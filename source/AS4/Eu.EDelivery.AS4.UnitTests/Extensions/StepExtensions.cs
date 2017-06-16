using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;

namespace Eu.EDelivery.AS4.UnitTests.Extensions
{
    public static class StepExtensions
    {
        /// <summary>
        /// Exercises the step.
        /// </summary>
        /// <param name="sut">The sut.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static async Task<StepResult> ExerciseStep(this IStep sut, MessagingContext context)
        {
            return await sut.ExecuteAsync(context, CancellationToken.None);
        }
    }
}