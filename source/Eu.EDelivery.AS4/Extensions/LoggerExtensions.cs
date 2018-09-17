using System;
using NLog;

namespace Eu.EDelivery.AS4.Extensions
{
    internal static class LoggerExtensions
    {
        public static void ErrorDeep(this ILogger logger, Exception exception)
        {
            logger.Error(exception.Message);
            if (exception.InnerException != null)
            {
                logger.Error(exception.InnerException.Message);
            }
        }
    }
}
