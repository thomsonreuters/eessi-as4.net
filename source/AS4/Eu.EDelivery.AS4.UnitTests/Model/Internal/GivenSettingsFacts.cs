using System.IO;
using System.Text;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Model.Internal;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Model.Internal
{
    /// <summary>
    /// Testing <see cref="Settings"/>
    /// </summary>
    public class GivenSettingsFacts
    {
        public class Defaults
        {
            [Fact]
            public void SettingsAreRunningPayloadInProcessByDefault()
            {
                // Act
                var settings = new Settings();

                // Assert
                Assert.True(settings.FeInProcess, "settings.FeInProcess");
                Assert.True(settings.PayloadServiceInProcess, "settings.PayloadServiceInProcess");
            }
        }

        public class PullReceiveAgent
        {
            [Fact]
            public void GetsExpectedPullReceiveAgentsCount()
            {
                // Act
                Settings settings = GetDeserializedSettings();

                // Assert
                SettingsAgent[] agents = GetPullReceiveAgents(settings);
                Assert.Equal(1, agents?.Length);
            }

            [Fact]
            public void GetsExpectedReceiverAttributeCount()
            {
                // Act
                Settings settings = GetDeserializedSettings();

                // Assert
                Setting[] receiverSetting = GetReceiverSetting(settings);
                Assert.Equal(2, receiverSetting?.Length);
                Assert.Equal(2, receiverSetting?[0].Attributes.Length);
            }

            [Fact]
            public void GetsExpectedReceiverAttributeValue()
            {
                // Act
                Settings settings = GetDeserializedSettings();

                // Assert
                Setting[] receiverSetting = GetReceiverSetting(settings);
                Assert.Equal("0:00:01", receiverSetting[0]["tmin"].Value);
            }
        }

        private static Setting[] GetReceiverSetting(Settings settings)
        {
            SettingsAgent[] agents = GetPullReceiveAgents(settings);
            return agents?[0].Receiver.Setting;
        }

        private static SettingsAgent[] GetPullReceiveAgents(Settings settings)
        {
            return settings?.Agents.PullReceiveAgents;
        }

        private static Settings GetDeserializedSettings()
        {
            using (var memoryStream = new MemoryStream(SerializedSettings()))
            using (var streamReader = new StreamReader(memoryStream))
            {
                var serializer = new XmlSerializer(typeof(Settings));
                return serializer.Deserialize(streamReader) as Settings;
            }
        }

        private static byte[] SerializedSettings()
        {
            return
                Encoding.UTF8.GetBytes(
                    "<?xml version=\"1.0\" encoding=\"utf-16\"?>"
                    + "<Settings xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"eu:edelivery:as4\">"
                    + "<Agents><PullReceiveAgent><Receiver Type=\"ExponentialConfiguredPModeReceiver\">"
                    + "<Setting key=\"pmode1\" tmin=\"0:00:01\" tmax=\"0:00:25\"/>"
                    + "<Setting key=\"pmode2\" tmin=\"0:00:05\" tmax=\"0:00:50\"/>"
                    + "</Receiver><Transformer Type=\"PModeToPullMessageTransformer\"/><Steps></Steps></PullReceiveAgent></Agents></Settings>");
        }
    }
}