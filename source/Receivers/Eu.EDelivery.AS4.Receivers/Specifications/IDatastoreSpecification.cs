using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Receivers.Specifications
{
    internal interface IDatastoreSpecification
    {
        string FriendlyExpression { get; }

        void Configure(DatastoreSpecificationArgs args);

        Expression<Func<DatastoreContext, IEnumerable<Entity>>> GetExpression();
    }
}