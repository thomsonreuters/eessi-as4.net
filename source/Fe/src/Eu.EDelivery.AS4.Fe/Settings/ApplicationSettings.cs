using System.Collections.Generic;

namespace Eu.EDelivery.AS4.Fe.Settings
{
    public class ApplicationSettings
    {
        public bool ShowStackTraceInExceptions { get; set; }
        public Dictionary<string, string> Modules { get; set; }
        public string SettingsXml { get; set; }
        public string Runtime { get; set; }
    }
}