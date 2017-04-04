using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.IntegrationTests.Common
{
    public class AS4Component
    {
        public static FileInfo SubmitSinglePayloadImage => new FileInfo(Path.GetFullPath(@".\" + Properties.Resources.submitmessage_single_payload_path));

        public static FileInfo SubmitSecondPayloadXml => new FileInfo(Path.GetFullPath($".{Properties.Resources.submitmessage_second_payload_path}"));
    }
}
