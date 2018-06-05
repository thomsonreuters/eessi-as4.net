using System;

namespace Eu.EDelivery.AS4.Security.Signing
{
    /// <summary>
    /// Wrapper for the Header and Body Signing ID Information
    /// </summary>
    public class SigningId
    {
        public string HeaderSecurityId { get; } = $"header-{Guid.NewGuid()}";

        public string BodySecurityId { get; } = $"body-{Guid.NewGuid()}";

        public SigningId() { }

        public SigningId(string headerSecurityId, string bodySecurityId)
        {
            if (!String.IsNullOrWhiteSpace(headerSecurityId))
            {
                HeaderSecurityId = headerSecurityId;
            }

            if (!String.IsNullOrWhiteSpace(bodySecurityId))
            {
                BodySecurityId = bodySecurityId;
            }
        }
    }
}