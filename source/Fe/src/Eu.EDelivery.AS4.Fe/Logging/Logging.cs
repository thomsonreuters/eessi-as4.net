using System;
using NLog;

namespace Eu.EDelivery.AS4.Fe.Logging
{
    public class Logging : ILogging
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public void Error(string message)
        {
            logger.Log(LogLevel.Error, message);
        }

        public void Error(Exception exception)
        {
            logger.Error(exception);
        }

        public void Info(string message)
        {
            logger.Log(LogLevel.Info, message);
        }

        public void Debug(string message)
        {
            logger.Log(LogLevel.Debug, message);
        }
    }
}