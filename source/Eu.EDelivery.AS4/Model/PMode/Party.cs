using System.Collections.Generic;
using System.Linq;

namespace Eu.EDelivery.AS4.Model.PMode
{
    public class Party
    {
        public List<PartyId> PartyIds { get; set; }
        
        public string Role { get; set; }

        public bool IsEmpty()
        {
            return
                string.IsNullOrEmpty(Role) && (PartyIds == null || PartyIds.Count == 0 || PartyIds.All(p => p.IsEmpty()));
        }

        /// <summary>
        /// Gets the primary party identifier of this <see cref="Party"/>'s <see cref="PartyId"/>.
        /// </summary>
        /// <value>The primary party identifier.</value>
        public string PrimaryPartyId => PartyIds?.FirstOrDefault()?.Id;

        /// <summary>
        /// Gets the type of the primary party of this <see cref="Party"/>'s <see cref="PartyId"/>.
        /// </summary>
        /// <value>The type of the primary party.</value>
        public string PrimaryPartyType => PartyIds?.FirstOrDefault()?.Type;

        /// <summary>
        /// Initializes a new instance of the <see cref="Party"/> class.
        /// </summary>
        public Party()
        {
            PartyIds = new List<PartyId>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Party"/> class.
        /// </summary>
        public Party(PartyId partyId) : this(null, partyId)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Party"/> class.
        /// </summary>
        public Party(string role, PartyId partyId)
        {
            Role = role;
            PartyIds = new List<PartyId> { partyId };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Party"/> class.
        /// </summary>
        public Party(string role, string partyId) : this(role, new PartyId { Id = partyId }) { }
    }
}