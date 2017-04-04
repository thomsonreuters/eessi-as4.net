using System;
using System.IO;

namespace Eu.EDelivery.AS4.PayloadService.Infrastructure
{
    internal class FilenameSanitizer
    {
        public static string EnsureValidFilename(string filename)
        {
            return String.Join("", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
