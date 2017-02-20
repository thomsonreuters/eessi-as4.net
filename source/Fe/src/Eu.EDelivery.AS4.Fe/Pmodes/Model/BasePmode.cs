using Eu.EDelivery.AS4.Fe.Hash;

namespace Eu.EDelivery.AS4.Fe.Pmodes.Model
{
    public class BasePmode<TPmode>
    {
        public PmodeType Type { get; set; }
        public string Name { get; set; }
        public TPmode Pmode { get; set; }
        public string Hash => Pmode.GetMd5Hash();
    }
}