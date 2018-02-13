using System;
using System.Linq;
using System.Threading;
using System.Xml;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
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

            const string settingsXml = @".\config\settings.xml";
            var doc = new XmlDocument();
            doc.Load(settingsXml);

            XmlElement retentionNode = doc.CreateElement(nameof(Settings.RetentionPeriod), "eu:edelivery:as4");

            // Retention Period in Days
            retentionNode.InnerText = 1.ToString();
            doc.DocumentElement?.AppendChild(retentionNode);
            doc.Save(settingsXml);
        }

        [Fact]
        public void MessageOlderThanRetentionDateWillBeDeleted()
        {
            // Arrange: Insert a "retired" OutMessage with a referenced Reception Awareness.
            Config config = Config.Instance;
            config.Initialize();

            string ebmsMessageId = Guid.NewGuid().ToString();

            var databaseSpy = new DatabaseSpy(config);
            databaseSpy.InsertOutMessage(
                CreateOutMessage(ebmsMessageId, insertionTime: DateTimeOffset.UtcNow.AddDays(-2)));

            InsertReferencedReceptionAwareness(config, ebmsMessageId);

            // Act: AS4.NET Component will start the Clean Up Agent.
            _msh = AS4Component.Start(Environment.CurrentDirectory);

            // Assert: No OutMessage or Reception Awareness entries must be found for a given EbmsMessageId.
            // Wait till AS4.NET Component has cleaned up the Messages Tables.
            Thread.Sleep(TimeSpan.FromSeconds(2));

            Assert.Null(databaseSpy.GetOutMessageFor(m => m.EbmsMessageId.Equals(ebmsMessageId)));
            Assert.Null(GetReferencedReceptionAwareness(config, ebmsMessageId));
        }

        private static OutMessage CreateOutMessage(string ebmsMessageId, DateTimeOffset insertionTime)
        {
            return new OutMessage(ebmsMessageId: ebmsMessageId)
            {
                MessageLocation = Registry.Instance.MessageBodyStore.SaveAS4Message(Config.Instance.InMessageStoreLocation, AS4Message.Empty),
                InsertionTime = insertionTime
            };
        }

        private static void InsertReferencedReceptionAwareness(IConfig config, string ebmsMessageId)
        {
            using (var ctx = new DatastoreContext(config))
            {
                ctx.ReceptionAwareness.Add(new ReceptionAwareness
                {
                    InternalMessageId = ebmsMessageId
                });
                ctx.SaveChanges();
            }
        }

        private static ReceptionAwareness GetReferencedReceptionAwareness(IConfig config, string ebmsMessageId)
        {
            using (var ctx = new DatastoreContext(config))
            {
                return ctx.ReceptionAwareness.FirstOrDefault(r => r.InternalMessageId.Equals(ebmsMessageId));
            }
        }

        protected override void Disposing(bool isDisposing)
        {
            _msh.Dispose();
        }
    }
}

