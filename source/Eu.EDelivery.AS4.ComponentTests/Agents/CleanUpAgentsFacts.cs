using System;
using System.Threading;
using System.Xml;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Xunit;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class CleanUpAgentFacts : ComponentTestTemplate
    {
        private AS4Component _msh;

        /// <summary>
        /// Initializes a new instance of the <see cref="CleanUpAgentFacts"/> class.
        /// </summary>
        public CleanUpAgentFacts()
        {
            OverrideSettings("no_agents_settings.xml");

            string retention = TimeSpan.FromDays(1).ToString("c");

            const string settingsXml = @".\config\settings.xml";
            var doc = new XmlDocument();
            doc.Load(settingsXml);

            XmlElement retentionNode = doc.CreateElement("RetentionPeriod", "eu:edelivery:as4");
            retentionNode.InnerText = retention;
            doc.DocumentElement?.AppendChild(retentionNode);
            doc.Save(settingsXml);
        }

        [Fact]
        public void MessageOlderThanRetentionDateWillBeDeleted()
        {
            // Arrange
            Config config = Config.Instance;
            config.Initialize();

            string ebmsMessageId = Guid.NewGuid().ToString();

            var databaseSpy = new DatabaseSpy(config);
            databaseSpy.InsertOutMessage(
                new OutMessage(ebmsMessageId: ebmsMessageId)
                {
                    MessageLocation = Registry.Instance.MessageBodyStore.SaveAS4Message(Config.Instance.InMessageStoreLocation, AS4Message.Empty),
                    InsertionTime = DateTimeOffset.UtcNow.AddDays(-2)
                });

            databaseSpy.InsertOutMessage(
                new OutMessage(ebmsMessageId: Guid.NewGuid().ToString())
                {
                    MessageLocation = Registry.Instance.MessageBodyStore.SaveAS4Message(Config.Instance.InMessageStoreLocation, AS4Message.Empty),
                    InsertionTime = DateTimeOffset.UtcNow.AddDays(-2)
                });

            // Act
            _msh = AS4Component.Start(Environment.CurrentDirectory);

            // Assert
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Assert.Null(databaseSpy.GetOutMessageFor(m => m.EbmsMessageId.Equals(ebmsMessageId)));
        }

        protected override void Disposing(bool isDisposing)
        {
            _msh.Dispose();
        }
    }
}
