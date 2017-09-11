using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Services.PullRequestAuthorization;

namespace Eu.EDelivery.AS4.UnitTests.Services
{
    internal class StubAuthorizationMapProvider : IPullAuthorizationMapProvider
    {
        private readonly IEnumerable<PullRequestAuthorizationEntry> _authorizationEntries;

        /// <summary>
        /// Initializes a new instance of the <see cref="StubAuthorizationMapProvider"/> class.
        /// </summary>
        public StubAuthorizationMapProvider(IEnumerable<PullRequestAuthorizationEntry> entries)
        {
            _authorizationEntries = entries;
        }

        public IEnumerable<PullRequestAuthorizationEntry> RetrievePullRequestAuthorizationEntriesForMpc(string mpc)
        {
            return _authorizationEntries.Where(e => e.Mpc == mpc).ToArray();
        }

        public void SavePullRequestAuthorizationEntries(IEnumerable<PullRequestAuthorizationEntry> entries)
        {
            throw new NotSupportedException();
        }

        public IEnumerable<PullRequestAuthorizationEntry> GetPullRequestAuthorizationEntryOverview()
        {
            return _authorizationEntries;
        }
    }
}
