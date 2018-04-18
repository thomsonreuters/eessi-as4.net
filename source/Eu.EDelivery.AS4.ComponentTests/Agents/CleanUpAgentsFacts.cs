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

        [Theory]
        [InlineData("no_agents_settings-sqlite.xml")]
        [InlineData("no_agents_settings-sqlserver.xml")]
        public void MessageOlderThanRetentionDateWillBeDeleted(string specificSettings)
        {
            // Arrange: Insert a "retired" OutMessage with a referenced Reception Awareness.
            OverrideWithSpecificSettings(specificSettings, retentionDays: 1);
            AS4Component.CleanupWorkingDirectory(Environment.CurrentDirectory);

            Config config = Config.Instance;
            config.Initialize("settings.xml");
            EnsureDatastoreCreated(config);

            DateTimeOffset overdueTime = DateTimeOffset.UtcNow.AddDays(-2);
            string outReferenceId = GenId(), outStandaloneId = GenId(),
                   inMessageId = GenId(), outExceptionId = GenId(),
                   inExceptionId = GenId();
            
            var spy = new DatabaseSpy(config);
            spy.InsertOutMessage(CreateOutMessage(outReferenceId, insertionTime: overdueTime));
            InsertReferencedReceptionAwareness(config, outReferenceId);
            spy.InsertOutMessage(CreateOutMessage(outStandaloneId, insertionTime: overdueTime));
            spy.InsertInMessage(CreateInMessage(inMessageId, overdueTime));
            spy.InsertOutException(CreateOutException(outExceptionId, overdueTime));
            spy.InsertInException(CreateInException(inExceptionId, overdueTime));

            // Act: AS4.NET Component will start the Clean Up Agent.
            ExerciseStartCleaning();

            // Assert: No OutMessage or Reception Awareness entries must be found for a given EbmsMessageId.
            Assert.Empty(spy.GetOutMessages(outReferenceId, outStandaloneId));
            Assert.Empty(spy.GetInMessages(inMessageId));
            Assert.Empty(spy.GetOutExceptions(outExceptionId));
            Assert.Empty(spy.GetInExceptions(inExceptionId));
            Assert.Null(GetReferencedReceptionAwareness(config, outReferenceId));
        }

        private void OverrideWithSpecificSettings(string settingsFile, int retentionDays)
        {
            OverrideSettings(settingsFile);

            const string settingsXml = @".\config\settings.xml";
            var doc = new XmlDocument();
            doc.Load(settingsXml);

            XmlElement retentionNode = doc.CreateElement(nameof(Settings.RetentionPeriod), "eu:edelivery:as4");

            // Retention Period in Days
            retentionNode.InnerText = retentionDays.ToString();
            doc.DocumentElement?.AppendChild(retentionNode);
            doc.Save(settingsXml);
        }

        private static void EnsureDatastoreCreated(Config config)
        {
            using (var ctx = new DatastoreContext(config))
            {
                ctx.NativeCommands.CreateDatabase().Wait();
            }
        }

        private static string GenId()
        {
            return Guid.NewGuid().ToString();
        }

        private static OutMessage CreateOutMessage(string ebmsMessageId, DateTimeOffset insertionTime)
        {
            return new OutMessage(ebmsMessageId: ebmsMessageId)
            {
                MessageLocation = 
                    Registry.Instance.MessageBodyStore.SaveAS4Message(
                        Config.Instance.InMessageStoreLocation, 
                        AS4Message.Empty),
                InsertionTime = insertionTime
            };
        }

        private static InMessage CreateInMessage(string ebmsMessageId, DateTimeOffset insertionTime)
        {
            return new InMessage(ebmsMessageId)
            {
                MessageLocation = Registry.Instance.MessageBodyStore.SaveAS4Message(
                    Config.Instance.InMessageStoreLocation,
                    AS4Message.Empty),
                InsertionTime = insertionTime
            };
        }

        private static OutException CreateOutException(string ebmsMessageId, DateTimeOffset insertionTime)
        {
            return new OutException(ebmsMessageId, errorMessage: string.Empty)
            {
                InsertionTime = insertionTime
            };
        }

        private static InException CreateInException(string ebmsMessageId, DateTimeOffset insertionTime)
        {
            return new InException(ebmsMessageId, errorMessage: string.Empty)
            {
                InsertionTime = insertionTime
            };
        }

        private static void InsertReferencedReceptionAwareness(IConfig config, string ebmsMessageId)
        {
            using (var ctx = new DatastoreContext(config))
            {
                long outMessageId = ctx.OutMessages.First(m => m.EbmsMessageId.Equals(ebmsMessageId)).Id;

                var ra = new ReceptionAwareness(outMessageId, ebmsMessageId);
                ra.SetStatus(ReceptionStatus.Completed);

                ctx.ReceptionAwareness.Add(ra);
                ctx.SaveChanges();
            }
        }
         
        private void ExerciseStartCleaning()
        {
            _msh = AS4Component.Start(Environment.CurrentDirectory, cleanSlate: false);

            // Wait till AS4.NET Component has cleaned up the Messages Tables.
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        private static ReceptionAwareness GetReferencedReceptionAwareness(IConfig config, string ebmsMessageId)
        {
            using (var ctx = new DatastoreContext(config))
            {
                return ctx.ReceptionAwareness.FirstOrDefault(r => r.RefToEbmsMessageId.Equals(ebmsMessageId));
            }
        }

        protected override void Disposing(bool isDisposing)
        {
            _msh.Dispose();
        }
    }
}