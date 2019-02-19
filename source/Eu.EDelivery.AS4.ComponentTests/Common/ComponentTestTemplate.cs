using System;
using System.Data.SqlClient;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.TestUtils;
using Polly;
using Xunit;

namespace Eu.EDelivery.AS4.ComponentTests.Common
{
    [Collection(ComponentTestCollection.ComponentTestCollectionName)]
    public class ComponentTestTemplate : IDisposable
    {
        private bool _restoreSettings = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentTestTemplate"/> class.
        /// </summary>
        public ComponentTestTemplate()
        {
            ClearLogFiles();
        }

        private static void ClearLogFiles()
        {
            if (Directory.Exists(@".\logs"))
            {
                foreach (string file in Directory.GetFiles(@".\logs"))
                {
                    Policy.Handle<IOException>()
                          .Retry(3)
                          .Execute(() => File.Delete(file));
                }
            }
        }

        public string ComponentTestSettingsPath => @".\config\componenttest-settings";

        protected Settings OverrideSettings(string settingsFile)
        {
            Console.WriteLine($@"Overwrite 'settings.xml' with '{settingsFile}'");

            File.Copy(@".\config\settings.xml", @".\config\settings_original.xml", true);

            string specificSettings = $@".\config\componenttest-settings\{settingsFile}";
            File.Copy(specificSettings, @".\config\settings.xml", true);

            _restoreSettings = true;

            return AS4XmlSerializer.FromString<Settings>(File.ReadAllText(specificSettings));
        }

        protected Settings OverrideServiceSettings(string settingsFile)
        {
            File.Copy(@".\config\settings-service.xml", @".\config\settings_service_original.xml", true);

            string specifiedSettings = $@".\config\componenttest-settings\{settingsFile}";
            File.Copy(specifiedSettings, @".\config\settings-service.xml", true);

            _restoreSettings = true;
            
            return AS4XmlSerializer.FromString<Settings>(File.ReadAllText(specifiedSettings));
        }

        protected async Task TestComponentWithSettings(string settingsFile, Func<Settings, AS4Component, Task> testCase)
        {
            Settings settings = OverrideSettings(settingsFile);

            using (var as4Msh = AS4Component.Start(Environment.CurrentDirectory))
            {
                await testCase(settings, as4Msh);
            }
        }

        protected Task<TResult> PollUntilPresent<TResult>(Func<TResult> poll, TimeSpan timeout)
        {
            IObservable<TResult> polling =
                Observable.Create<TResult>(o =>
                {
                    TResult r = poll();
                    Console.WriteLine($@"Poll until present: {(r == null ? "(null)" : r.ToString())}");

                    IObservable<TResult> observable =
                        r == null
                            ? Observable.Throw<TResult>(new Exception())
                            : Observable.Return(r);
                    return observable.Subscribe(o);
                });

            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);

            return Observable
                       .Timer(TimeSpan.FromSeconds(1))
                       .SelectMany(_ => polling)
                       .Retry()
                       .ToTask(cts.Token);
        }

        protected Task PollUntilSatisfied(Func<bool> poll, TimeSpan timeout)
        {
            var polling =
                Observable.Create<bool>(o =>
                {
                    bool result = poll();
                    Console.WriteLine($@"Poll until satisfied - result = {result}");

                    IObservable<bool> observable =
                        result == false
                            ? Observable.Throw<bool>(new Exception())
                            : Observable.Return<bool>(true);
                    return observable.Subscribe(o);
                });

            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);

            return Observable
                .Timer(TimeSpan.FromSeconds(1))
                .SelectMany(_ => polling)
                .Retry()
                .ToTask(cts.Token);
        }

        public void Dispose()
        {
            Disposing(true);
            if (_restoreSettings && File.Exists(@".\config\settings_original.xml"))
            {
                File.Copy(@".\config\settings_original.xml", @".\config\settings.xml", true);
            }

            if (_restoreSettings && File.Exists(@".\config\settings_service_original.xml"))
            {
                File.Copy(@".\config\settings_service_original.xml", @".\config\settings-service.xml", true);
            }

            AS4Component.WriteLogFilesToConsole();
        }

        protected virtual void Disposing(bool isDisposing) { }
    }

    [CollectionDefinition(ComponentTestCollectionName)]
    public class ComponentTestCollection : ICollectionFixture<ComponentTestFixture>
    {
        public const string ComponentTestCollectionName = "ComponentTestCollection";
    }

    public class ComponentTestFixture
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentTestFixture"/> class.
        /// </summary>
        public ComponentTestFixture()
        {
            FileSystemUtils.CreateOrClearDirectory(@".\config\send-pmodes");
            FileSystemUtils.CreateOrClearDirectory(@".\config\receive-pmodes");
            FileSystemUtils.CreateOrClearDirectory(@".\messages\in");
            FileSystemUtils.CreateOrClearDirectory(@".\messages\out");
            FileSystemUtils.CreateOrClearDirectory(@".\messages\receipts");
            FileSystemUtils.CreateOrClearDirectory(@".\messages\errors");
            FileSystemUtils.CreateOrClearDirectory(@".\messages\exceptions");

            FileSystemUtils.CopyDirectory(@".\config\componenttest-settings\send-pmodes", @".\config\send-pmodes");
            FileSystemUtils.CopyDirectory(@".\config\componenttest-settings\receive-pmodes", @".\config\receive-pmodes");

            FileSystemUtils.CopyDirectory(@".\samples\pmodes\", @".\config\receive-pmodes", "*receive-pmode.xml");

            DropComponentTestSqlServerDatabases();
        }

        private static void DropComponentTestSqlServerDatabases()
        {
            var settingFiles = Directory.GetFiles(@".\config\componenttest-settings", "*.xml");

            foreach (var setting in settingFiles)
            {
                DropSqlServerDatabase(setting);
            }
        }

        private static void DropSqlServerDatabase(string settingFile)
        {
            string xml = File.ReadAllText(settingFile);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);

            XmlNode connectionStringNode = xmlDocument.SelectSingleNode("//*[local-name()='ConnectionString']");
            if (connectionStringNode == null)
            {
                Console.WriteLine($"No '<ConnectionString/>' node found in settings file: {settingFile}");
                return;
            }

            string mshConnectionString = connectionStringNode.InnerText;

            // Modify the connectionstring so that we initially connect to the master - database.
            // Otherwise, the connection will fail if the database doesn't exist yet.
            SqlConnectionStringBuilder builder;

            try
            {
                builder = new SqlConnectionStringBuilder(mshConnectionString);
            }
            catch (ArgumentException)
            {
                Console.WriteLine($"Connectionstring in {settingFile} is not a SqlServer connectionstring");
                return;
            }

            if (builder.DataSource != ".")
            {
                throw new InvalidOperationException("Not allowed to drop a database that does not reside on the local server");
            }

            string databaseName = builder.InitialCatalog;
            builder.InitialCatalog = "master";

            string masterConnectionString = builder.ConnectionString;
            using (var sqlConnection = new SqlConnection(masterConnectionString))
            {
                sqlConnection.Open();

                var cmd = new SqlCommand($"DROP DATABASE IF EXISTS {databaseName}", sqlConnection);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
