using System;
using System.Collections.Concurrent;
using System.IO;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Watchers
{
    /// <summary>
    /// Watcher to check if there's a new Sending PMode available
    /// </summary>
    public class PModeWatcher<T> where T : class, IPMode
    {
        private readonly ConcurrentDictionary<string, T> _pmodes;

        public PModeWatcher(string path, ConcurrentDictionary<string, T> pmodes)
        {
            this._pmodes = pmodes;

            var watcher = new FileSystemWatcher(path, "*.xml");
            watcher.Changed += OnChanged;
            watcher.Created += OnCreated;
            watcher.EnableRaisingEvents = true;
            watcher.NotifyFilter = GetNotifyFilters();

            RetrievePModes(watcher.Path);
        }

        private void RetrievePModes(string pmodeFolder)
        {
            var startDir = new DirectoryInfo(pmodeFolder);

            FileInfo[] files = startDir.GetFiles("*.xml", SearchOption.AllDirectories);
            foreach (FileInfo file in files) AddOrUpdatePMode(file.FullName);
        }

        private NotifyFilters GetNotifyFilters()
        {
            return NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            AddOrUpdatePMode(e.FullPath);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            AddOrUpdatePMode(e.FullPath);
        }

        private void AddOrUpdatePMode(string fullPath)
        {
            T pmode = TryDeserialize(fullPath);
            if (pmode != null) this._pmodes.AddOrUpdate(pmode.Id, s => pmode, (s, t) => pmode);
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