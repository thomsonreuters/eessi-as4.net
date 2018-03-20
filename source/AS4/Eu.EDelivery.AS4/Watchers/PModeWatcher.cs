using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Validators;
using FluentValidation;
using FluentValidation.Results;
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
        private readonly ConcurrentDictionary<string, string> _filePModeIdMap = new ConcurrentDictionary<string, string>();

        private readonly AbstractValidator<T> _pmodeValidator;
        private readonly FileSystemWatcher _watcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="PModeWatcher{T}" /> class
        /// </summary>
        /// <param name="path">The path on which this watcher should look for <see cref="IPMode"/> implementations.</param>
        /// <param name="validator">The validator to use when retrieving <see cref="IPMode"/> implementations.</param>
        public PModeWatcher(string path, AbstractValidator<T> validator)
        {
            _pmodeValidator = validator;

            _watcher = new FileSystemWatcher(path, "*.xml") { IncludeSubdirectories = true };
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

        /// <summary>
        /// Gets the <see cref="IPMode"/> implementation for a given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">The specified PMode key is invalid. - key</exception>
        public IPMode GetPMode(string key)
        {
           return GetPModeEntry(key)?.PMode;
        }

        /// <summary>
        /// Gets the <see cref="ConfiguredPMode"/> entry for a given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">The specified PMode key is invalid. - key</exception>
        public ConfiguredPMode GetPModeEntry(string key)
        {
            if (String.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(@"The specified PMode key is invalid.", nameof(key));
            }

            _pmodes.TryGetValue(key, out ConfiguredPMode configuredPMode);
            return configuredPMode;
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
            AddOrUpdateConfiguredPMode(Path.GetFullPath(e.FullPath));
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            AddOrUpdateConfiguredPMode(Path.GetFullPath(e.FullPath));
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            string key = _pmodes.FirstOrDefault(p => p.Value.Filename.Equals(e.FullPath)).Key;

            if (key != null)
            {
                LogManager.GetCurrentClassLogger().Trace($"Remove {typeof(T).Name} with Id: " + key);
                _pmodes.TryRemove(key, out _);
            }
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

            T pmode = TryDeserialize(fullPath);
            if (pmode == null)
            {
                LogManager.GetCurrentClassLogger().Warn("File at: '" + fullPath + "' cannot be converted to a PMode because the XML in the file isn't valid.");

                // Since the PMode that we expect in this file is invalid, it
                // must be removed from our cache.
                RemoveLocalPModeFromCache(fullPath);
                return;
            }

            ValidationResult pmodeValidation = _pmodeValidator.Validate(pmode);
            if (!pmodeValidation.IsValid)
            {
                LogManager.GetCurrentClassLogger().Warn("Invalid PMode at: '" + fullPath + "'");
                pmodeValidation.LogErrors(LogManager.GetCurrentClassLogger());

                // Since the PMode that we expect isn't valid according to the validator, it
                // must be removed from our cache.
                RemoveLocalPModeFromCache(fullPath);
                return;
            }

            var configuredPMode = new ConfiguredPMode(fullPath, pmode);

            if (_pmodes.ContainsKey(pmode.Id))
            {
                LogManager.GetCurrentClassLogger().Warn($"There already exists a configured PMode with id {pmode.Id}.");
                LogManager.GetCurrentClassLogger()
                          .Warn($"Existing PMode will be overwritten with PMode from {fullPath}");
            }
            else
            {
                LogManager.GetCurrentClassLogger().Trace($"Add new {typeof(T).Name} with Id: " + pmode.Id);
            }

            _pmodes.AddOrUpdate(pmode.Id, configuredPMode, (key, value) => configuredPMode);
            _filePModeIdMap.AddOrUpdate(fullPath, pmode.Id, (key, value) => pmode.Id);
        }

        // cache which keeps track of the date and time a PMode file was last handled by the FileSystemWatcher.

        // Due to an issue with FileSystemWatcher, events can be triggered multiple times for the same operation on the 

        // same file.

        private readonly MemoryCache _fileEventCache = MemoryCache.Default;

        private readonly CacheItemPolicy _policy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMilliseconds(500) };

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

        private void RemoveLocalPModeFromCache(string fullPath)
        {
            if (_filePModeIdMap.TryGetValue(fullPath, out string pmodeId))
            {
                _pmodes.TryRemove(pmodeId, out _);
                _filePModeIdMap.TryRemove(fullPath, out _);
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
                _filePModeIdMap.Clear();                
                _watcher?.Dispose();
            }
        }

    }
}