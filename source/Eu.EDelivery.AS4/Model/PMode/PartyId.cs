using System;

namespace Eu.EDelivery.AS4.Model.PMode
{
    public class PartyId
    {
        public string Id { get; set; }
        public string Type { get; set; }

        public bool IsEmpty()
        {
            return String.IsNullOrWhiteSpace(Id) &&
                   String.IsNullOrWhiteSpace(Type);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyId"/> class.
        /// </summary>
        public PartyId()
        {
            
        }

        public PartyId(string id)
        {
            Id = id;
        }
    }
}