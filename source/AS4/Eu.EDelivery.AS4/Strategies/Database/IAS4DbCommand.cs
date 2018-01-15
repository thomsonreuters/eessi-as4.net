using System.Collections.Generic;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Strategies.Database
{
    public interface IAS4DbCommand
    {
        /// <summary>
        /// Exclusively retrieves the entities.
        /// </summary>
        /// <param name="tableName">Name of the Db table.</param>
        /// <param name="filter">Order by this field.</param>
        /// <param name="takeRows">Take this amount of rows.</param>
        /// <returns></returns>
        IEnumerable<Entity> ExclusivelyRetrieveEntities(
            string tableName,
            string filter,
            int takeRows);
    }
}
