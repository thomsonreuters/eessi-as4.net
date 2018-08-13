using System.Collections.Generic;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Method
{
    public class LocationMethod : AS4.Model.PMode.Method
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocationMethod"/> class.
        /// </summary>
        /// <param name="location">The location.</param>
        public LocationMethod(string location)
        {
            Parameters = new List<Parameter> {new Parameter {Name = "location", Value = location}};
        }
    }
}