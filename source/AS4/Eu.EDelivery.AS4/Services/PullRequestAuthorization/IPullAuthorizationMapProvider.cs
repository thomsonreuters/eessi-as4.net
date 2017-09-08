using System.Collections.Generic;

namespace Eu.EDelivery.AS4.Services.PullRequestAuthorization
{
    public interface IPullAuthorizationMapProvider
    {
        IEnumerable<PullRequestAuthorizationEntry> RetrievePullRequestAuthorizationEntriesForMpc(string mpc);
        void SavePullRequestAuthorizationEntries(IEnumerable<PullRequestAuthorizationEntry> entries);
    }
}
