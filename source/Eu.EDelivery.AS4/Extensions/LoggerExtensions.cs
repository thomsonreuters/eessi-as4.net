using System;
using log4net;

namespace Eu.EDelivery.AS4.Extensions
{
    public static class LoggerExtensions
    {
        public static void ErrorDeep(this ILog logger, Exception exception)
        {
            logger.Error(exception.Message);
            if (exception.InnerException != null)
            {
                logger.Error(exception.InnerException.Message);
            }
        }
        public static void Trace(this ILog log, string message, Exception exception)
        {
            log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                log4net.Core.Level.Trace, message, exception);
        }

        public static void Trace(this ILog log, Exception exception, string message)
        {
            log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                log4net.Core.Level.Trace, message, exception);
        }

        public static void Trace(this ILog log, string message)
        {
            log.Trace(message, null);
        }
    }
}
