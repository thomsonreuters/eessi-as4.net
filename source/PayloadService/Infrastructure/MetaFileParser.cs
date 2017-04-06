using System;
using System.IO;

namespace Eu.EDelivery.AS4.PayloadService.Infrastructure
{
    internal class MetaFileParser
    {
        private const string OriginalFileNameKey = "originalfilename:";

        internal static PayloadMeta Parse(string metafile)
        {
            string[] lines = File.ReadAllLines(metafile);

            string originalFilename = string.Empty; 

            foreach (string line in lines)
            {
                if (line.IndexOf(OriginalFileNameKey, StringComparison.CurrentCultureIgnoreCase) > -1)
                {
                    originalFilename = line.Substring(OriginalFileNameKey.Length);
                }
            }

            return new PayloadMeta(originalFilename);
        }
    }

    internal class PayloadMeta
    {
        public string OriginalFileName { get; private set; }

        public PayloadMeta(string originalFileName)
        {
            OriginalFileName = originalFileName;
        }
    }

}