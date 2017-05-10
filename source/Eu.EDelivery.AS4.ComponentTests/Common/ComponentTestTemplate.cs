using System;
using System.IO;
using Xunit;

namespace Eu.EDelivery.AS4.ComponentTests.Common
{
    [Collection("ComponentTest")]
    public class ComponentTestTemplate : IDisposable
    {
        private bool _restoreSettings = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentTestTemplate"/> class.
        /// </summary>
        public ComponentTestTemplate()
        {
            CopyDirectory(@".\config\componenttest-settings\send-pmodes", @".\config\send-pmodes");
        }

        protected void OverrideSettings(string settingsFile)
        {
            File.Copy(@".\config\settings.xml", @".\config\settings_original.xml", true);
            File.Copy($@".\config\componenttest-settings\{settingsFile}", @".\config\settings.xml", true);
            _restoreSettings = true;
        }

        public void Dispose()
        {
            Disposing(true);
            if (_restoreSettings && File.Exists(@".\config\settings_original.xml"))
            {
                File.Copy(@".\config\settings_original.xml", @".\config\settings.xml", true);
            }
        }

        protected virtual void Disposing(bool isDisposing) { }

        #region Infrastructure methods

        private static void CopyDirectory(string sourceDirName, string destDirName)
        {
            if (Directory.Exists(sourceDirName) == false)
            {
                throw new DirectoryNotFoundException($"The {sourceDirName} directory can not be found.");
            }

            if (Directory.Exists(destDirName) == false)
            {
                Directory.CreateDirectory(destDirName);
            }

            var files = Directory.GetFiles(sourceDirName);

            foreach (string fileName in files)
            {
                File.Copy(fileName, Path.Combine(destDirName, Path.GetFileName(fileName)), true);
            }
        }

        #endregion
    }
}
