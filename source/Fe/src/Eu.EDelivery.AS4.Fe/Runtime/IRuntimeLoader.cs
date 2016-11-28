using System.Collections.Generic;

namespace Eu.EDelivery.AS4.Fe.Runtime
{
    public interface IRuntimeLoader
    {
        IEnumerable<ItemType> Receivers { get; }
        IEnumerable<ItemType> Steps { get; }
        IEnumerable<ItemType> Transformers { get; }
        IRuntimeLoader Initialize();
    }
}