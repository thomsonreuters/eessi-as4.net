using System;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.Common
{
    public interface IRegistry
    {
        bool IsInitialized { get; }
        Func<DatastoreContext> CreateDatastoreContext { get; }
        ICertificateRepository CertificateRepository { get; }
        MessageBodyStore MessageBodyStore { get; }
    }
}