using System;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Services
{
    public class GivenAuthorizationMapFacts
    {
        public class AuthorizedFacts
        {
            [Fact]
            public void IfMpcMatchesCertificate()
            {
                throw new NotImplementedException();
            }

            [Fact]
            public void IfMpcIfNoEntriesExistForMpcInAuthorizationMap()
            {
                throw new NotImplementedException();
            }

            [Fact]
            public void IfPullRequestIsNotSignedAndNoEntriesExistForMpcInAuthorizationMap()
            {
                throw new NotImplementedException();
            }

        }

        public class NotAuthorizedFacts
        {
            [Fact]
            public void IfMpcIsNotDefinedInAuthorizationMap()
            {
                throw new NotImplementedException();
            }

            [Fact]
            public void IfPullRequestIsNotSignedAndEntriesExistInAuthorizationMap()
            {
                throw new NotImplementedException();
            }
        }

    }
}
