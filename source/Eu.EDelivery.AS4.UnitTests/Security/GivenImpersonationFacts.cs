using System;
using Eu.EDelivery.AS4.Security;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Security
{
    /// <summary>
    /// Testing <seealso cref="AS4.Security.Impersonation" />\
    /// </summary>
    public class GivenImpersonationFacts
    {
        private readonly string _user, _password;

        public GivenImpersonationFacts()
        {
            this._user = "admin";
            this._password = "123";
        }

        /// <summary>
        /// Testing if the Impersonation succeeds
        /// </summary>
        public class GivenImpersonationSucceeds
            : GivenImpersonationFacts {}

        /// <summary>
        /// Testing if the Impersonation fails
        /// </summary>
        public class GivenImpersonationFails
            : GivenImpersonationFacts
        {
            [Fact]
            public void ThenImpersonateFails()
            {
                // Act / Assert
                Assert.Throws<InvalidOperationException>(
                    () =>
                        Impersonation.Impersonate(this._user, this._password));
            }
        }
    }
}