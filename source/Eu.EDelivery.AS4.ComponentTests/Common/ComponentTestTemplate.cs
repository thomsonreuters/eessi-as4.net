using System;
using System.IO;
using Eu.EDelivery.AS4.TestUtils;
using Xunit;

namespace Eu.EDelivery.AS4.ComponentTests.Common
{
    [Collection("ComponentTest")]
    public class ComponentTestTemplate : IClassFixture<ComponentTestFixture>, IDisposable
    {
        private bool _restoreSettings = false;

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


        #endregion
    }

    public class ComponentTestFixture
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentTestFixture"/> class.
        /// </summary>
        public ComponentTestFixture()
        {
            FileSystemUtils.ClearDirectory(@".\config\send-pmodes");
            FileSystemUtils.ClearDirectory(@".\config\receive-pmodes");

            FileSystemUtils.CopyDirectory(@".\config\componenttest-settings\send-pmodes", @".\config\send-pmodes");
            FileSystemUtils.CopyDirectory(@".\config\componenttest-settings\receive-pmodes", @".\config\receive-pmodes");
        }

    }
}
