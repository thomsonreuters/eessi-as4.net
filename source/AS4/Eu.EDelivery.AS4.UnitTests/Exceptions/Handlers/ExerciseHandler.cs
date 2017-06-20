using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.UnitTests.Exceptions.Handlers
{
    public static class ExerciseHandler
    {
        /// <summary>
        /// Exercises the transform exception.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <param name="createContext">The create context.</param>
        /// <param name="contents">The contents.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public static async Task<MessagingContext> ExerciseTransformException(
            this IAgentExceptionHandler handler,
            Func<DatastoreContext> createContext,
            string contents,
            Exception exception)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
            {
                stream.Position = 1;
                return await handler.HandleTransformationException(stream, exception); 
            }
        }
    }
}
