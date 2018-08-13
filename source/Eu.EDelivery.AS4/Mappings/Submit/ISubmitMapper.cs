using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Submit;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// Interface to describe the mapping of <see cref="SubmitMessage"/> properties
    /// </summary>
    public interface ISubmitMapper
    {
        void Map(SubmitMessage submitMessage, UserMessage userMessage);
    }
}
