using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Eu.EDelivery.AS4.Exceptions
{
    /// <summary>
    /// Base Exception for all the Exceptions inside the AS4 Project
    /// </summary>
    [Serializable]
    public class AS4Exception : Exception
    {
        private readonly List<string> _messageIds = new List<string>();

        public AS4Exception(string description) : base(description) {}

        public ErrorCode ErrorCode { get; internal set; }

        public ErrorAlias ErrorAlias { get; internal set; }

        public string PMode { get; internal set; }

        public string[] MessageIds => _messageIds.ToArray();

        /// <summary>
        /// Add a Message Id to the <see cref="AS4Exception" /> Message Ids
        /// </summary>
        /// <param name="messageId"></param>
        public void AddMessageId(string messageId)
        {
            _messageIds.Add(messageId);
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with
        /// information about the exception.
        /// </summary>
        /// <param name="info">
        /// The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
        /// data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
        /// information about the source or destination.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="info" /> parameter is a null reference (Nothing in
        /// Visual Basic).
        /// </exception>
        /// <PermissionSet>
        /// <IPermission
        ///     class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///     version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
        /// <IPermission
        ///     class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///     version="1" Flags="SerializationFormatter" />
        /// </PermissionSet>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(ErrorCode), ErrorCode);
            info.AddValue(nameof(ErrorAlias), ErrorAlias);
            info.AddValue(nameof(PMode), PMode);
            info.AddValue(nameof(MessageIds), MessageIds, typeof(string[]));

            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Set the <paramref name="messageIds"/> collection inside the <see cref="AS4Exception"/>.
        /// </summary>
        /// <param name="messageIds">Referenced Message Ids</param>
        public void SetMessageIds(IEnumerable<string> messageIds)
        {
            _messageIds.Clear();
            _messageIds.AddRange(messageIds);
        }

        /// <summary>Creates and returns a string representation of the current exception.</summary>
        /// <returns>A string representation of the current exception.</returns>
        /// <filterpriority>1</filterpriority>
        /// <PermissionSet>
        /// <IPermission
        ///     class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, 
        /// Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///     version="1" PathDiscovery="*AllFiles*" />
        /// </PermissionSet>
        public override string ToString()
        {
            string innerExceptionMessage = string.Empty;

            Exception inner = InnerException;

            while (inner != null)
            {
                innerExceptionMessage = inner.Message;
                inner = inner.InnerException;
            }

            string descriptionPart = $"{Message}";
            if (!string.IsNullOrWhiteSpace(innerExceptionMessage))
            {
                descriptionPart += $": {innerExceptionMessage}";
            }

            return $"{descriptionPart}{Environment.NewLine}{StackTrace}{Environment.NewLine}{InnerException}";
        }
    }
}