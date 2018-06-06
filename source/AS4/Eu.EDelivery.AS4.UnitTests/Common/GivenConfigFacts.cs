using System;
using System.IO;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    public class GivenConfigFacts
    {
        [Fact]
        public void Initialize_With_Default_Retry_Polling_Interval()
        {
            TestConfigInitialization(
                alterSettings: settings =>
                    settings.RetryReliability = new SettingsRetryReliability { PollingInterval = null },
                onInitialized: config =>
                    Assert.Equal(
                        TimeSpan.FromSeconds(5), 
                        config.RetryPollingInterval));
        }

        private static void TestConfigInitialization(
            Action<Settings> alterSettings,
            Action<Config> onInitialized)
        {
            string testSettingsFileName = Path.Combine(
                Config.ApplicationPath, "config", "test-settings.xml");

            string originalSettingsFileName = Path.Combine(
                Config.ApplicationPath, "config", "settings.xml");

            var settings = AS4XmlSerializer
                .FromString<Settings>(File.ReadAllText(originalSettingsFileName));

            alterSettings(settings);

            File.WriteAllText(
                testSettingsFileName,
                AS4XmlSerializer.ToString(settings));

            File.Copy(
                originalSettingsFileName,
                testSettingsFileName,
                overwrite: true);

            // Act
            Config.Instance.Initialize(testSettingsFileName);

            // Assert
            onInitialized(Config.Instance);

            // TearDown
            File.Delete(testSettingsFileName);
        }
    }
}
