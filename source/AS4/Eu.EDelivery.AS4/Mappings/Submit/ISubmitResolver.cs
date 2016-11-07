using Eu.EDelivery.AS4.Model.Submit;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISubmitResolver<out T>
    {
        T Resolve(SubmitMessage submitMessage);
    }
}
