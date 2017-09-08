using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Eu.EDelivery.AS4.Services.PullRequestAuthorization
{
    public class FilePullAuthorizationMapProvider : IPullAuthorizationMapProvider
    {
        // TODO: use a FileSystemWatcher to determine if the file needs to be reloaded.

        private readonly string _authorizationMapFile;

        private bool _authorizationMapChanged;
        private IEnumerable<PullRequestAuthorizationEntry> _entries;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePullAuthorizationMapProvider"/> class.
        /// </summary>
        public FilePullAuthorizationMapProvider(string authorizationMapFile)
        {
            _authorizationMapFile = authorizationMapFile;
        }

        public IEnumerable<PullRequestAuthorizationEntry> RetrievePullRequestAuthorizationEntriesForMpc(string mpc)
        {
            if (_authorizationMapChanged || _entries == null)
            {
                _entries = RetrievePullRequestEntriesFromFile(_authorizationMapFile);
                _authorizationMapChanged = false;
            }

            return _entries.Where(e => StringComparer.InvariantCulture.Equals((string) e.Mpc, mpc)).ToArray();
        }

        public void SavePullRequestAuthorizationEntries(IEnumerable<PullRequestAuthorizationEntry> authorizationEntries)
        {
            var entries = new List<AuthorizationEntry>();

            foreach (var entry in authorizationEntries)
            {
                entries.Add(new AuthorizationEntry() { Mpc = entry.Mpc, CertificateThumbPrint = entry.CertificateThumbprint, Allowed = entry.Allowed });
            }

            var map = new PullRequestAuthorizationMap();
            map.AuthorizationEntries = entries.ToArray();

            using (var fs = new FileStream(_authorizationMapFile, FileMode.Create, FileAccess.Write))
            {
                var s = new XmlSerializer(typeof(PullRequestAuthorizationMap));
                s.Serialize(fs, map);
            }
        }

        private static IEnumerable<PullRequestAuthorizationEntry> RetrievePullRequestEntriesFromFile(string fileName)
        {
            if (File.Exists(fileName) == false)
            {
                yield break;
            }

            PullRequestAuthorizationMap map;

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer s = new XmlSerializer(typeof(PullRequestAuthorizationMap));
                map = (PullRequestAuthorizationMap)s.Deserialize(fs);
            }

            foreach (var entry in map.AuthorizationEntries)
            {
                yield return new PullRequestAuthorizationEntry(entry.Mpc, entry.CertificateThumbPrint, entry.Allowed);
            }
        }

        #region Inner classes for xml - serialization

        [Serializable]
        [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
        [XmlRoot("PullRequestAuthorizationMap")]
        public class PullRequestAuthorizationMap
        {
            [XmlArray("AuthorizationEntries")]
            [XmlArrayItem("Authorization")]
            public AuthorizationEntry[] AuthorizationEntries;
        }

        [Serializable]
        [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
        [XmlRoot("Authorization")]
        public class AuthorizationEntry
        {
            [XmlAttribute(AttributeName = "mpc")]
            public string Mpc { get; set; }
            [XmlAttribute(AttributeName = "certificatethumbprint")]
            public string CertificateThumbPrint { get; set; }
            [XmlAttribute(AttributeName = "allowed")]
            public bool Allowed { get; set; }
        }

        #endregion
    }
}