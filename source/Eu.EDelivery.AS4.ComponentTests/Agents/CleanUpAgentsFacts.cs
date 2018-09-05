using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Xml;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Config = Eu.EDelivery.AS4.Common.Config;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{

    public class CleanUpAgentFacts : ComponentTestTemplate
    {
        private static readonly DateTimeOffset DayBeforeYesterday = DateTimeOffset.Now.AddDays(-2);

        [Property(MaxTest = 1)]
        public Property Only_Awnsered_UserMessages_Are_Deleted()
        {
            return Prop.ForAll(
                SupportedProviderSettings(),
                Arb.From<OutStatus>(),
                (specificSettings, status) =>
                {
                    // Arrange
                    OverrideWithSpecificSettings(specificSettings);

                    string id = GenId();
                    OutMessage m = CreateOutMessage(
                        id, insertionTime: DayBeforeYesterday, type: MessageType.UserMessage);
                    m.SetStatus(status);

                    IConfig config = EnsureLocalConfigPointsToCreatedDatastore();
                    var spy = new DatabaseSpy(config);
                    spy.InsertOutMessage(m);

                    // Act
                    ExerciseStartCleaning();

                    // Assert
                    bool isCleaned = !spy.GetOutMessages(id).Any();
                    bool isAckOrNack = status == OutStatus.Ack || status == OutStatus.Nack;
                    return isCleaned == isAckOrNack;
                });
        }

        [Property(MaxTest = 1)]
        public Property Only_Entries_With_Allowed_Operations_Are_Deleted()
        {
            return Prop.ForAll(
                SupportedProviderSettings(),
                Arb.From<Operation>(),
                (specificSettings, operation) =>
                {
                    // Arrange
                    OverrideWithSpecificSettings(specificSettings);

                    string id = GenId();
                    InMessage m = CreateInMessage(id, insertionTime: DayBeforeYesterday);
                    m.Operation = operation;

                    IConfig config = EnsureLocalConfigPointsToCreatedDatastore();
                    var spy = new DatabaseSpy(config);
                    spy.InsertInMessage(m);

                    // Act
                    ExerciseStartCleaning();

                    // Assert
                    bool hasEntries = spy.GetInMessages(id).Any();
                    string description = $"InMessage {(hasEntries ? "isn't" : "is")} deleted, with Operation: {operation}";
                    return (hasEntries == !AllowedOperations.Contains(operation)).Collect(description);
                });
        }

        private static IEnumerable<Operation> AllowedOperations =>
            new[]
            {
                Operation.Delivered,
                Operation.Forwarded,
                Operation.Notified,
                Operation.Sent,
                Operation.NotApplicable,
                Operation.Undetermined
            };

        [Property(MaxTest = 1)]
        public Property Only_Overdue_Entries_Are_Deleted()
        {
            return Prop.ForAll(
                SupportedProviderSettings(),
                Arb.Default.PositiveInt(),
                Arb.Default.PositiveInt(),
                (specificSettings, insertion, retention) =>
                {
                    // Arrange
                    int retentionDays = retention.Get;
                    OverrideWithSpecificSettings(specificSettings, retentionDays: retentionDays);

                    int insertionDays = insertion.Get;
                    string id = GenId();

                    IConfig config = EnsureLocalConfigPointsToCreatedDatastore();
                    var spy = new DatabaseSpy(config);
                    var insertionTime = DateTimeOffset.Now.Add(TimeSpan.FromDays(-insertionDays));
                    spy.InsertOutException(CreateOutException(id, insertionTime));

                    // Act
                    ExerciseStartCleaning();

                    // Assert
                    bool hasEntries = spy.GetOutExceptions(id).Any();
                    return (hasEntries == insertionDays < retentionDays)
                           .When(insertionDays != retentionDays)
                           .Classify(hasEntries, "OutException isn't deleted")
                           .Classify(!hasEntries, "OutException is deleted");
                });
        }

        private static Arbitrary<string> SupportedProviderSettings()
        {
            return Gen.Elements(
                          "no_agents_settings-sqlite.xml",
                          "no_agents_settings-sqlserver.xml")
                      .ToArbitrary();
        }


        [Theory]
        [InlineData("no_agents_settings-sqlite.xml")]
        [InlineData("no_agents_settings-sqlserver.xml")]
        public void MessageOlderThanRetentionDateWillBeDeleted(string specificSettings)
        {
            // Arrange: Insert a "retired" OutMessage with a referenced Reception Awareness.
            OverrideWithSpecificSettings(specificSettings);

            IConfig config = EnsureLocalConfigPointsToCreatedDatastore();
            string outReferenceId = GenId(), outStandaloneId = GenId(),
                   inMessageId = GenId(), outExceptionId = GenId(),
                   inExceptionId = GenId();

            var spy = new DatabaseSpy(config);
            OutMessage om = CreateOutMessage(outReferenceId, insertionTime: DayBeforeYesterday, type: MessageType.Error);
            spy.InsertOutMessage(om);
            spy.InsertRetryReliability(RetryReliability.CreateForOutMessage(om.Id, maxRetryCount: 0, retryInterval: default(TimeSpan), type: RetryType.Send));
            spy.InsertOutMessage(CreateOutMessage(outStandaloneId, insertionTime: DayBeforeYesterday, type: MessageType.Receipt));
            spy.InsertInMessage(CreateInMessage(inMessageId, DayBeforeYesterday));
            spy.InsertOutException(CreateOutException(outExceptionId, DayBeforeYesterday));
            spy.InsertInException(CreateInException(inExceptionId, DayBeforeYesterday));
            
            // Act: AS4.NET Component will start the Clean Up Agent.
            ExerciseStartCleaning();

            // Assert: No OutMessage or Reception Awareness entries must be found for a given EbmsMessageId.
            Assert.Empty(spy.GetOutMessages(outReferenceId, outStandaloneId));
            Assert.Null(spy.GetRetryReliabilityFor(r => r.RefToOutMessageId == om.Id));
            Assert.Empty(spy.GetInMessages(inMessageId));
            Assert.Empty(spy.GetOutExceptions(outExceptionId));
            Assert.Empty(spy.GetInExceptions(inExceptionId));
        }

        private void OverrideWithSpecificSettings(string settingsFile, int retentionDays = 1)
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

        private static IConfig EnsureLocalConfigPointsToCreatedDatastore()
        {
            Config config = Config.Instance;
            config.Initialize("settings.xml");

            EnsureDatastoreCreated(config);
            EnsureCleanDatastore(config);

            return config;
        }

        private static void EnsureDatastoreCreated(IConfig config)
        {
            using (var ctx = new DatastoreContext(config))
            {
                ctx.NativeCommands.CreateDatabase().GetAwaiter().GetResult();
            }
        }

        private static void EnsureCleanDatastore(IConfig config)
        {
            using (var ctx = new DatastoreContext(config))
            {
                ctx.Database.ExecuteSqlCommand("DELETE FROM OutMessages");
                ctx.Database.ExecuteSqlCommand("DELETE FROM InMessages");
                ctx.Database.ExecuteSqlCommand("DELETE FROM OutExceptions");
                ctx.Database.ExecuteSqlCommand("DELETE FROM InExceptions");
            }
        }

        private static string GenId()
        {
            return Guid.NewGuid().ToString();
        }

        private static OutMessage CreateOutMessage(
            string ebmsMessageId,
            DateTimeOffset insertionTime,
            MessageType type)
        {
            var m = new OutMessage(ebmsMessageId: ebmsMessageId)
            {
                MessageLocation =
                    Registry.Instance.MessageBodyStore.SaveAS4Message(
                        @"file:///.\database\as4messages\out",
                        AS4Message.Empty),
                InsertionTime = insertionTime
            };

            m.EbmsMessageType = type;
            return m;
        }

        private static InMessage CreateInMessage(string ebmsMessageId, DateTimeOffset insertionTime)
        {
            return new InMessage(ebmsMessageId)
            {
                MessageLocation = Registry.Instance.MessageBodyStore.SaveAS4Message(
                    @"file:///.\database\as4messages\in",
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

        private static void ExerciseStartCleaning()
        {
            var msh = AS4Component.Start(Environment.CurrentDirectory, cleanSlate: false);

            // Wait till AS4.NET Component has cleaned up the Messages Tables.
            Thread.Sleep(TimeSpan.FromSeconds(2));

            msh.Dispose();
        }

        protected override void Disposing(bool isDisposing)
        {
            foreach (var process in Process.GetProcessesByName("Eu.EDelivery.AS4.ServiceHandler.ConsoleHost"))
            {
                if (process.HasExited)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception)
                    {
                        // Ignore
                    }
                }
            }
        }
    }
}