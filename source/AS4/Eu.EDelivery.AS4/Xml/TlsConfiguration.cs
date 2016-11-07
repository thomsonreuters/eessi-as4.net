using System;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Xml
{
    /// <summary>
    /// Adding Tls Version to the TlsConfiguration
    /// </summary>
    public partial class TlsConfiguration
    {
        private TlsVersion _tlsVersion = TlsVersion.Tls12;

        [XmlElement("TlsVersion")]
        public string TlsVersionString
        {
            get { return this._tlsVersion.ToString(); }
            set { this._tlsVersion = (TlsVersion)Enum.Parse(typeof(TlsVersion), value); }
        }

        [XmlIgnore]
        public TlsVersion TlsVersion
        {
            get { return this._tlsVersion; }
            set { this._tlsVersion = value; }
        }
    }
}
