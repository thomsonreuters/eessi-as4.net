using System;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Steps.Notify
{
    /// <summary>
    /// Assemble a <see cref="AS4Message"/> as Notify Message
    /// </summary>
    [Obsolete("Has been replaced by a Transformer")]
    public class ConformanceTestCreateNotifyMessageStep : MinderCreateNotifyMessageStep
    {
        // TODO: this step should be replaced by a Transformer

        protected override string MinderUriPrefix
        {
            get { return "http://www.esens.eu/as4/conformancetest"; }
        }
    }
}