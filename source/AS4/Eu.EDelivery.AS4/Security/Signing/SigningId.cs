using System;

namespace Eu.EDelivery.AS4.Security.Signing
{
    /// <summary>
    /// Wrapper for the Header and Body Signing ID Information
    /// </summary>
    public class SigningId
    {
        private string _headerSecurityId = $"header-{Guid.NewGuid()}";
        private string _bodySecurityId = $"body-{Guid.NewGuid()}";

        public string HeaderSecurityId
        {
            get { return this._headerSecurityId; }
            set { this._headerSecurityId = value ?? this._headerSecurityId; }
        }

        public string BodySecurityId
        {
            get { return this._bodySecurityId; }
            set { this._bodySecurityId = value ?? this._bodySecurityId; }
        }

        public SigningId() {}

        public SigningId(string headerSecurityId, string bodySecurityId)
        {
            this.HeaderSecurityId = headerSecurityId;
            this.BodySecurityId = bodySecurityId;
        }
    }
}