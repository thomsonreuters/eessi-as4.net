using Eu.EDelivery.AS4.Model.PMode;
using Newtonsoft.Json.Linq;

namespace Eu.EDelivery.AS4.Fe.Pmodes.Model
{

    public class ReceivingBasePmode : BasePmode<ReceivingProcessingMode>
    {
        private ReceivingProcessingMode pmode;

        /// <summary>
        /// Gets or sets the pmode.
        /// </summary>
        /// <value>
        /// The pmode.
        /// </value>
        public override ReceivingProcessingMode Pmode
        {
            get => pmode;
            set
            {
                pmode = value;
                if (value?.Security?.Decryption == null || !(value.Security.Decryption.DecryptCertificateInformation is JObject json)) return;
                value.Security.Decryption.DecryptCertificateInformation = json.ToObject<CertificateFindCriteria>();
            }
        }
    }
}