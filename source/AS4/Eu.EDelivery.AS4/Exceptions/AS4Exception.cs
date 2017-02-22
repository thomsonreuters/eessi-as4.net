using System;
using System.Collections.Generic;

namespace Eu.EDelivery.AS4.Exceptions
{
    /// <summary>
    /// Base Exception for all the Exceptions inside the AS4 Project
    /// </summary>
    [Serializable]
    public class AS4Exception : Exception
    {
        public ErrorCode ErrorCode { get; internal set; }
        public ExceptionType ExceptionType { get; internal set; }
        public string PMode { get; internal set; }
        public string[] MessageIds {
            get { return _messageIds.ToArray(); }
        }

        public void SetMessageIds(IEnumerable<string> messageIds)
        {
            _messageIds.Clear();
            _messageIds.AddRange(messageIds);
        }

        private readonly List<string> _messageIds = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AS4Exception"/> class. 
        /// Create a new <see cref="AS4Exception"/>
        /// </summary>
        /// <param name="description">
        /// </param>
        internal AS4Exception(string description) : base(description) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="AS4Exception"/> class. 
        /// Create a new <see cref="AS4Exception"/>
        /// with a given <paramref name="message"/> and <paramref name="innerException"/>
        /// </summary>
        /// <param name="message">
        /// </param>
        /// <param name="innerException">
        /// </param>
        internal AS4Exception(string message, Exception innerException) : base(message, innerException) {}

        /// <summary>
        /// Add a Message Id to the <see cref="AS4Exception"/> Message Ids
        /// </summary>
        /// <param name="messageId"></param>
        public void AddMessageId(string messageId)
        {
            _messageIds.Add(messageId);            
        }

        /// <summary>Creates and returns a string representation of the current exception.</summary>
        /// <returns>A string representation of the current exception.</returns>
        /// <filterpriority>1</filterpriority>
        /// <PermissionSet>
        ///     <IPermission
        ///         class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, 
        /// Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///         version="1" PathDiscovery="*AllFiles*" />
        /// </PermissionSet>
        public override string ToString()
        {
            return $"{this.Message}{Environment.NewLine}{this.StackTrace}{Environment.NewLine}{this.InnerException}";
        }
    }
}