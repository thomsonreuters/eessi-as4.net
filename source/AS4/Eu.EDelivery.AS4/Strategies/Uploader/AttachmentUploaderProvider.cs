using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Eu.EDelivery.AS4.Exceptions;

namespace Eu.EDelivery.AS4.Strategies.Uploader
{
    /// <summary>
    /// Class to provide the right <see cref="IAttachmentUploader"/> implementation
    /// </summary>
    public class AttachmentUploaderProvider : IAttachmentUploaderProvider
    {
        private readonly ICollection<UploaderEntry> _uploaders;

        /// <summary>
        /// Initializes a new instance of the type <see cref="AttachmentUploaderProvider"/> class
        /// </summary>
        public AttachmentUploaderProvider()
        {
            this._uploaders = new Collection<UploaderEntry>();
        }

        /// <summary>
        /// Get the right <see cref="IAttachmentUploader"/> implementation
        /// for a given <paramref name="type"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IAttachmentUploader Get(string type)
        {
            UploaderEntry entry = this._uploaders.FirstOrDefault(u => u.Condition(type));

            if(entry?.Uploader == null)
                throw new AS4Exception($"No Attachment Uploader found for Type: {type}");

            return entry.Uploader;
        }

        /// <summary>
        /// Adds a new <see cref="IAttachmentUploader"/> implementation
        /// for a given <paramref name="condition"/>
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="uploader"></param>
        public void Accept(Func<string, bool> condition, IAttachmentUploader uploader)
        {
            this._uploaders.Add(new UploaderEntry(condition, uploader));
        }

        private class UploaderEntry
        {
            public Func<string, bool> Condition { get;}
            public IAttachmentUploader Uploader { get; }

            public UploaderEntry(Func<string, bool> condition, IAttachmentUploader uploader)
            {
                this.Condition = condition;
                this.Uploader = uploader;
            }
        }
    }

    public interface IAttachmentUploaderProvider
    {
        /// <summary>
        /// Accept a <see cref="IAttachmentUploader"/> implementation in the <see cref="IAttachmentUploaderProvider"/>.
        /// </summary>
        /// <param name="condition">Condition for which the <see cref="IAttachmentUploader"/> must be used.</param>
        /// <param name="uploader"><see cref="IAttachmentUploader"/> implementation to be used.</param>
        void Accept(Func<string, bool> condition, IAttachmentUploader uploader);

        /// <summary>
        /// Get a <see cref="IAttachmentUploader"/> implementation based on a given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type for which the <see cref="IAttachmentUploader"/> implementation is accepted.</param>
        /// <returns></returns>
        IAttachmentUploader Get(string type);
    }
}
