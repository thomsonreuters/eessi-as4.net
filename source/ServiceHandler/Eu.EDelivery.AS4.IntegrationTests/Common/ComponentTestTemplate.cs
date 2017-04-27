using System;
using System.IO;
using Xunit;

namespace Eu.EDelivery.AS4.IntegrationTests.Common
{
    [Collection("ComponentTest")]
    public class ComponentTestTemplate : IDisposable
    {
        private bool _restoreSettings = false;

        protected void OverrideSettings(string settingsFile)
        {
            File.Copy(@".\config\settings.xml", @".\config\settings_original.xml", true);
            File.Copy(settingsFile, @".\config\settings.xml", true);
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
    }
}
