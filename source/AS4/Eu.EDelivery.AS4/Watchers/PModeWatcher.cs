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
        }

        private NotifyFilters GetNotifyFilters()
        {
            return NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            AddOrUpdatePMode(e);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            AddOrUpdatePMode(e);
        }

        private void AddOrUpdatePMode(FileSystemEventArgs e)
        {
            T pmode = TryDeserialize(e.FullPath);
            this._pmodes.AddOrUpdate(pmode.Id, s => pmode, (s, t) => pmode);
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