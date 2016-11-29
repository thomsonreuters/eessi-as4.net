using System.Collections.Generic;

namespace Eu.EDelivery.AS4.Fe.Runtime
{
    public class ItemType
    {
        public string Name { get; set; }
        public IEnumerable<Property> Properties { get; set; }
        public string TechnicalName { get; internal set; }
    }
}