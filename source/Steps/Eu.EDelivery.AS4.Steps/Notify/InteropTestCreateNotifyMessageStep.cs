namespace Eu.EDelivery.AS4.Steps.Notify
{
    public class InteropTestCreateNotifyMessageStep : MinderCreateNotifyMessageStep
    {
        // TODO: this step should be replaced by a Transformer
        protected override string MinderUriPrefix
        {
            get { return "http://www.esens.eu/as4/interoptest"; }
        }
    }
}