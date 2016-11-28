using System;

namespace Eu.EDelivery.AS4.Fe.Logging
{
    public interface ILogging
    {
        void Error(string message);
        void Error(Exception exception);
        void Info(string message);
        void Debug(string message);
    }
}
