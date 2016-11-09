using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;

namespace Eu.EDelivery.AS4.Watchers
{
    /// <summary>
    /// Watcher to check if there's a new Sending PMode available
    /// </summary>
    public class PModeWatcher<T> where T : class, IPMode
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, ConfiguredPMode> _pmodes;

        /// <summary>
        /// Initializes a new instance of the <see cref="PModeWatcher{T}"/> class
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pmodes"></param>
        public PModeWatcher(string path, ConcurrentDictionary<string, ConfiguredPMode> pmodes)
        {
            this._pmodes = pmodes;
            this._logger = LogManager.GetCurrentClassLogger();

            var watcher = new FileSystemWatcher(path, "*.xml");
            watcher.Changed += OnChanged;
            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.EnableRaisingEvents = true;
            watcher.NotifyFilter = GetNotifyFilters();

            RetrievePModes(watcher.Path);
        }

        private void RetrievePModes(string pmodeFolder)
        {
            var startDir = new DirectoryInfo(pmodeFolder);

            FileInfo[] files = startDir.GetFiles("*.xml", SearchOption.AllDirectories);
            foreach (FileInfo file in files) AddOrUpdateConfiguredPMode(file.FullName);
        }

        private NotifyFilters GetNotifyFilters()
        {
            return NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            AddOrUpdateConfiguredPMode(e.FullPath);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            AddOrUpdateConfiguredPMode(e.FullPath);
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            string key = this._pmodes.FirstOrDefault(p => p.Value.Filename.Equals(e.FullPath)).Key;

            ConfiguredPMode pmode = null;
            this._pmodes.TryRemove(key, out pmode);
        }

        private void AddOrUpdateConfiguredPMode(string fullPath)
        {
            IPMode pmode = TryDeserialize(fullPath);
            if(pmode == null) return;

            var configuredPMode = new ConfiguredPMode(fullPath, pmode);
            this._pmodes.AddOrUpdate(pmode.Id, configuredPMode, (key, value) => configuredPMode);
        }

        private T TryDeserialize(string path)
        {
            try
            {
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    return serializer.Deserialize(fileStream) as T;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}