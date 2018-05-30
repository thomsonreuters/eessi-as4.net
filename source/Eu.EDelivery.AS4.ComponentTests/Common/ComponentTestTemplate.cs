using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.TestUtils;
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
            AS4Mapper.Initialize();
        }

        public string ComponentTestSettingsPath => @".\config\componenttest-settings";

        protected Settings OverrideSettings(string settingsFile)
        {
            File.Copy(@".\config\settings.xml", @".\config\settings_original.xml", true);

            string specificSettings = $@".\config\componenttest-settings\{settingsFile}";
            File.Copy(specificSettings, @".\config\settings.xml", true);

            _restoreSettings = true;

            return AS4XmlSerializer.FromString<Settings>(File.ReadAllText(specificSettings));
        }

        protected Task<TResult> PollUntilPresent<TResult>(Func<TResult> poll, TimeSpan timeout)
        {
            IObservable<TResult> polling =
                Observable.Create<TResult>(o =>
                {
                    TResult r = poll();
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

    public void Dispose()
        {
            Disposing(true);
            if (_restoreSettings && File.Exists(@".\config\settings_original.xml"))
            {
                File.Copy(@".\config\settings_original.xml", @".\config\settings.xml", true);
            }
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
        }

    }
}
