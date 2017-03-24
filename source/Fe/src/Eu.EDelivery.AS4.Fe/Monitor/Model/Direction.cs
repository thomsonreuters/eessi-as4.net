using System;

namespace Eu.EDelivery.AS4.Fe.Monitor.Model
{
    [Flags]
    public enum Direction : int
    {
        Inbound = 0,
        Outbound = 1
    }
}