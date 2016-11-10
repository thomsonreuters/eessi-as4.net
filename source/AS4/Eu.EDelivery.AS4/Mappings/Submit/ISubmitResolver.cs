using Eu.EDelivery.AS4.Model.Submit;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// Interface to describe what to resolve of the <see cref="SubmitMessage"/>
    /// </summary>
    public interface ISubmitResolver<out T>
    {
        T Resolve(SubmitMessage submitMessage);
    }
}
