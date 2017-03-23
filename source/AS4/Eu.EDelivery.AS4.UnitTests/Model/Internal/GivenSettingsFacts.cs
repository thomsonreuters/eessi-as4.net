using System;
using System.Collections;
using System.Collections.Generic;
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
        public class PullReceiveAgent : GivenSettingsFacts
        {
            [Theory]
            [ClassData(typeof(PullReceiveAgentAssertion))]
            public void SerializesPullReceiveAgent(Action<Settings> assertionAction)
            {
                using (var memoryStream = new MemoryStream(SerializedSettings()))
                using (var streamReader = new StreamReader(memoryStream))
                {
                    // Arrange
                    var serializer = new XmlSerializer(typeof(Settings));

                    // Act
                    var settings = serializer.Deserialize(streamReader) as Settings;

                    // Assert
                    assertionAction(settings);
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

        private class PullReceiveAgentAssertion : IEnumerable<object[]>
        {
            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {new Action<Settings>(AssertReceiveAgent)};
                yield return new object[] {new Action<Settings>(AssertReceiverAttributes)};
            }

            private static void AssertReceiveAgent(Settings settings)
            {
                SettingsAgent[] agents = GetPullReceiveAgents(settings);
                Assert.Equal(1, agents?.Length);
            }

            private static void AssertReceiverAttributes(Settings settings)
            {
                SettingsAgent[] agents = GetPullReceiveAgents(settings);
                Setting[] receiverSetting = agents?[0].Receiver.Setting;

                Assert.Equal(2, receiverSetting?.Length);
                Assert.Equal(2, receiverSetting?[0].Attributes.Length);
            }

            private static SettingsAgent[] GetPullReceiveAgents(Settings settings)
            {
                return settings?.Agents.PullReceiveAgents;
            }
        }
    }
}