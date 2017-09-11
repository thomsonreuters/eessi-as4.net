using System.IO;

namespace Eu.EDelivery.AS4.IntegrationTests.Common
{
    public class HolodeckLocations
    {
        public string InputPath { get; }
        public string OutputPath { get; }
        public string PModePath { get; }
        public string DbPath { get; }
        public string BinaryPath { get; }
        public string XmlPayloadPath { get; }
        public string JpegPayloadPath { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HolodeckLocations"/> class.
        /// </summary>
        public HolodeckLocations(string rootLocation)
        {
            InputPath = Path.Combine(rootLocation, @"data\msg_in");
            OutputPath = Path.Combine(rootLocation, @"data\msg_out");
            PModePath = Path.Combine(rootLocation, @"conf\pmodes");
            DbPath = Path.Combine(rootLocation, @"db");
            XmlPayloadPath = Path.Combine(rootLocation, @"data\msg_out\payloads\simple_document.xml");
            JpegPayloadPath = Path.Combine(rootLocation, @"data\msg_out\payloads\dandelion.jpg");

            BinaryPath = Path.Combine(rootLocation, @"bin\startserver.bat");
        }

        public static HolodeckLocations ProbeForHolodeckInstance(string holodeckPath)
        {
            bool Probe(string probePath, out HolodeckLocations location)
            {
                string holodeckExecutable = Path.Combine(probePath, $@"{holodeckPath}\bin\startserver.bat");
                if (File.Exists(holodeckExecutable) == false)
                {
                    location = null;
                    return false;
                }

                location = new HolodeckLocations(Path.Combine(probePath, holodeckPath));
                return true;
            }

            HolodeckLocations probedLocation;

            if (Probe(@"c:\holodeck", out probedLocation))
            {
                return probedLocation;
            }

            if (Probe(@"C:\Program Files\Java\holodeck", out probedLocation))
            {
                return probedLocation;
            }

            if (Probe(@"C:\Program Files (x86)\Java\holodeck", out probedLocation))
            {
                return probedLocation;
            }

            return null;
        }
    }
}