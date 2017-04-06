using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.PayloadService.Persistance
{
    public interface IPayloadPersister
    {
        Guid SavePayload();
    }
}
