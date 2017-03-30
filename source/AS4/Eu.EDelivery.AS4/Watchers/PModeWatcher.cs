using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;

namespace Eu.EDelivery.AS4.Watchers
{
    /// <summary>
    /// Watcher to check if there's a new Sending PMode available
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PModeWatcher<T> : IDisposable where T : class, IPMode
    {
        private readonly ConcurrentDictionary<string, ConfiguredPMode> _pmodes = new ConcurrentDictionary<string, ConfiguredPMode>(StringComparer.OrdinalIgnoreCase);

        private readonly FileSystemWatcher _watcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="PModeWatcher{T}"/> class
        /// </summary>
        /// <param name="path"></param>        
        public PModeWatcher(string path)
        {
            _watcher = new FileSystemWatcher(path, "*.xml") {IncludeSubdirectories = true};
            _watcher.Changed += OnChanged;
            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;
            _watcher.NotifyFilter = GetNotifyFilters();

            RetrievePModes(_watcher.Path);
        }

        public void Start()
        {
            _watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            _watcher.EnableRaisingEvents = false;
        }

        /// <summary>
        /// Verify if the Watcher contains a <see cref="IPMode"/> for a given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Id for which the verification is done.</param>
        /// <returns></returns>
        public bool ContainsPMode(string id)
        {
            return _pmodes.ContainsKey(id);
        }

        public IPMode GetPMode(string key)
        {
            if (String.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(@"The specified PMode key is invalid.", nameof(key));
            }

            ConfiguredPMode configuredPMode;

            this._pmodes.TryGetValue(key, out configuredPMode);

            return configuredPMode?.PMode;
        }

        public IEnumerable<IPMode> GetPModes()
        {
            return _pmodes.Values.Select(p => p.PMode);
        }

        private void RetrievePModes(string pmodeFolder)
        {
            var startDir = new DirectoryInfo(pmodeFolder);
            IEnumerable<FileInfo> files = TryGetFiles(startDir);

            foreach (FileInfo file in files)
            {
                AddOrUpdateConfiguredPMode(file.FullName);
            }
        }

        private static IEnumerable<FileInfo> TryGetFiles(DirectoryInfo startDir)
        {
            try
            {
                return startDir.GetFiles("*.xml", SearchOption.AllDirectories);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error($"An error occured while trying to get PMode files: {ex.Message}");
                return new List<FileInfo>();
            }
        }

        private static NotifyFilters GetNotifyFilters()
        {
            return NotifyFilters.LastAccess | NotifyFilters.LastWrite |
                    NotifyFilters.FileName | NotifyFilters.DirectoryName;
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
            ConfiguredPMode pmode;

            string key = this._pmodes.FirstOrDefault(p => p.Value.Filename.Equals(e.FullPath)).Key;
            if (key != null) this._pmodes.TryRemove(key, out pmode);
        }

        private readonly object _cacheLock = new object();

        private void AddOrUpdateConfiguredPMode(string fullPath)
        {
            lock (_cacheLock)
            {
                if (_fileEventCache.Contains(fullPath))
                {
                    LogManager.GetCurrentClassLogger().Trace($"PMode {fullPath} has already been handled.");
                    return;
                }

                _fileEventCache.Add(fullPath, fullPath, _policy);
            }

            IPMode pmode = TryDeserialize(fullPath);
            if (pmode == null)
            {
                return;
            }

            var configuredPMode = new ConfiguredPMode(fullPath, pmode);

            if (this._pmodes.ContainsKey(pmode.Id))
            {
                LogManager.GetCurrentClassLogger().Warn($"There already exists a configured PMode with id {pmode.Id}.");
                LogManager.GetCurrentClassLogger().Warn($"Existing PMode will be overwritten with PMode from {fullPath}");
            }

            this._pmodes.AddOrUpdate(pmode.Id, configuredPMode, (key, value) => configuredPMode);
        }

        // cache which keeps track of the date and time a PMode file was last handled by the FileSystemWatcher.
        // Due to an issue with FileSystemWatcher, events can be triggered multiple times for the same operation on the 
        // same file.
        private readonly System.Runtime.Caching.MemoryCache _fileEventCache = System.Runtime.Caching.MemoryCache.Default;
        private readonly System.Runtime.Caching.CacheItemPolicy _policy = new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromMilliseconds(500) };

        private static T TryDeserialize(string path)
        {
            try
            {
                return Deserialize(path);
            }
            catch (Exception ex)
            {
                var logger = LogManager.GetCurrentClassLogger();
                logger.Error($"An error occured while deserializing PMode {path}");
                logger.Error(ex.Message);
                if (ex.InnerException != null)
                {
                    logger.Error(ex.InnerException.Message);
                }
                return null;
            }
        }

        private static bool IsFileLocked(string path)
        {
            try
            {
                using (File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
            }
        }

        private static T Deserialize(string path)
        {
            int retryCount = 0;

            while (IsFileLocked(path) && retryCount < 10)
            {
                // Wait till the filelock is released ...
                System.Threading.Thread.Sleep(50);
                retryCount++;
            }

            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var serializer = new XmlSerializer(typeof(T));
                return serializer.Deserialize(fileStream) as T;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _pmodes.Clear();
                _watcher?.Dispose();
            }
        }

    }
}