using System;
using System.IO;

namespace Eu.EDelivery.AS4.Utilities
{
    public class FilenameUtils
    {
        public static string EnsureValidFilename(string filename)
        {
            return string.Join(string.Empty, filename.Split(Path.GetInvalidFileNameChars()));
        }

        public static string EnsureFilenameIsUnique(string filename)
        {
            while (File.Exists(filename))
            {
                const string copyExtension = " - Copy";

                string name = Path.GetFileNameWithoutExtension(filename) + copyExtension + Path.GetExtension(filename);
                string copyFilename = Path.Combine(Path.GetDirectoryName(filename) ?? string.Empty, name);

                filename = copyFilename;
            }

            return filename;
        }
    }
}
