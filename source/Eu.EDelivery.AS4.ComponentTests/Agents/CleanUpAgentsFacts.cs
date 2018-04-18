using System;
using System.Collections.Generic;
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
using Xunit.Abstractions;
using Config = Eu.EDelivery.AS4.Common.Config;
using XunitRunner = Eu.EDelivery.AS4.ComponentTests.Common.XunitRunner;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{

    public class CleanUpAgentFacts : ComponentTestTemplate
    {
        private readonly Configuration _testConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="CleanUpAgentFacts" /> class.
        /// </summary>
        /// <param name="outputHelper">The output helper.</param>
        public CleanUpAgentFacts(ITestOutputHelper outputHelper)
        {
            _testConfig = Configuration.VerboseThrowOnFailure;
            _testConfig.MaxNbOfTest = 5;
            _testConfig.Runner = new XunitRunner(outputHelper);
        }

        [Fact]
        public void Only_Entries_With_Allowed_Operations_Are_Deleted()
        {
            var operations = Gen.Elements(
                Enum.GetNames(typeof(Operation))
                    .Select(n => (Operation) Enum.Parse(typeof(Operation), n)))
                                .ToArbitrary();
            Prop.ForAll(
                SupportedProviderSettings(),
                operations,
                (specificSettings, operation) =>
                {
                    // Arrange
                    OverrideWithSpecificSettings(specificSettings, retentionDays: 1);

                    string id = GenId();
                    InMessage m = CreateInMessage(id, insertionTime: DateTimeOffset.UtcNow.AddDays(-2));
                    m.SetOperation(operation);

                    IConfig config = ParseLocalConfig();
                    EnsureDatastoreCreated(config);
                    EnsureCleanDatastore(config);

                    var spy = new DatabaseSpy(config);
                    spy.InsertInMessage(m);

                    // Act
                    ExerciseStartCleaning();

                    // Assert
                    bool hasEntries = spy.GetInMessages(id).Any();
                    string description = $"InMessage {(hasEntries ? "isn't" : "is")} deleted, with Operation: {operation}";
                    return (hasEntries == !AllowedOperations.Contains(operation)).Collect(description);
                }).Check(_testConfig);
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

        [Fact]
        public void Only_Overdue_Entries_Are_Deleted()
        {
            Prop.ForAll(
                SupportedProviderSettings(),
                Arb.Default.PositiveInt(),
                Arb.Default.PositiveInt(),
                (specificSettings, insertion, retention) =>
                {
                    // Arrange
                    int retentionDays = retention.Get;
                    OverrideWithSpecificSettings(specificSettings, retentionDays: retentionDays);
                    AS4Component.CleanupWorkingDirectory(Environment.CurrentDirectory);

                    int insertionDays = insertion.Get;
                    string id = GenId();

                    IConfig config = ParseLocalConfig();
                    EnsureDatastoreCreated(config);
                    EnsureCleanDatastore(config);

                    var spy = new DatabaseSpy(config);
                    var insertionTime = DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(-insertionDays));
                    spy.InsertOutMessage(CreateOutMessage(id, insertionTime));

                    // Act
                    ExerciseStartCleaning();

                    // Assert
                    bool hasEntries = spy.GetOutMessages(id).Any();
                    return (hasEntries == insertionDays < retentionDays)
                           .When(insertionDays != retentionDays)
                           .Classify(hasEntries, "OutMessage isn't deleted")
                           .Classify(!hasEntries, "OutMessage is deleted");
                }).Check(_testConfig);
        }

        private static Arbitrary<string> SupportedProviderSettings()
        {
            return Gen.Elements(
                          "no_agents_settings-sqlite.xml", 
                          "no_agents_settings-sqlserver.xml")
                      .ToArbitrary();
        }

        private static void EnsureCleanDatastore(IConfig config)
        {
            using (var ctx = new DatastoreContext(config))
            {
                ctx.Database.ExecuteSqlCommand("DELETE FROM OutMessages");
                ctx.Database.ExecuteSqlCommand("DELETE FROM InMessages");
            }
        }


        [Theory]
        [InlineData("no_agents_settings-sqlite.xml")]
        [InlineData("no_agents_settings-sqlserver.xml")]
        public void MessageOlderThanRetentionDateWillBeDeleted(string specificSettings)
        {
            // Arrange: Insert a "retired" OutMessage with a referenced Reception Awareness.
            OverrideWithSpecificSettings(specificSettings, retentionDays: 1);
            AS4Component.CleanupWorkingDirectory(Environment.CurrentDirectory);

            IConfig config = ParseLocalConfig();
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

        private static IConfig ParseLocalConfig()
        {
            Config config = Config.Instance;
            config.Initialize("settings.xml");
            return config;
        }

        private static void EnsureDatastoreCreated(IConfig config)
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
            var msh = AS4Component.Start(Environment.CurrentDirectory, cleanSlate: false);

            // Wait till AS4.NET Component has cleaned up the Messages Tables.
            Thread.Sleep(TimeSpan.FromSeconds(2));

            msh.Dispose();
        }

        private static ReceptionAwareness GetReferencedReceptionAwareness(IConfig config, string ebmsMessageId)
        {
            using (var ctx = new DatastoreContext(config))
            {
                return ctx.ReceptionAwareness.FirstOrDefault(r => r.RefToEbmsMessageId.Equals(ebmsMessageId));
            }
        }
    }
}