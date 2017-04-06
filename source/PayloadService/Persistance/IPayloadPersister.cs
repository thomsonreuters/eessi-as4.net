using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.PayloadService.Models;

namespace Eu.EDelivery.AS4.PayloadService.Persistance
{
    /// <summary>
    /// Contract to persist the Payload on a given specific implementation detail location.
    /// </summary>
    internal interface IPayloadPersister
    {
        /// <summary>
        /// Save the given <paramref name="payload"/> with its Metadata
        /// to a specific implementation detail location.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        Task<string> SavePayload(Payload payload);

        /// <summary>
        /// Load a <see cref="Payload"/> with a given <paramref name="payloadId"/> 
        /// from the specific implementation detail location.
        /// </summary>
        /// <param name="payloadId"></param>
        /// <returns></returns>
        Task<Payload> LoadPayload(string payloadId);
    }
}