using Eu.EDelivery.AS4.Model.PMode;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eu.EDelivery.AS4.Fe.Pmodes.Model
{
    public class SendingBasePmode : BasePmode<SendingProcessingMode>
    {
        private SendingProcessingMode pmode;

        public override SendingProcessingMode Pmode
        {
            get { return pmode; }
            set
            {
                pmode = value;
                if (value?.Security?.Encryption?.PublicKeyInformation is JObject json)
                {
                    if (json["certificate"] != null) value.Security.Encryption.PublicKeyInformation = json.ToObject<PublicKeyCertificate>();
                    else value.Security.Encryption.PublicKeyInformation = json.ToObject<PublicKeyFindCriteria>();
                }
            }
        }
    }
}