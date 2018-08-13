using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Mappings.PMode
{
    /// <summary>
    /// Interface to describe what to resolve from the <see cref="SendingProcessingMode"/>
    /// </summary>
    public interface IPModeResolver<out T>
    {
        T Resolve(SendingProcessingMode pmode);
    }
}
