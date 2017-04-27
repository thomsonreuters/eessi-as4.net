using System;
using System.IO;
using System.Reflection;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.Transforms;
using Eu.EDelivery.AS4.Streaming;
using MimeKit.IO;
using Reference = System.Security.Cryptography.Xml.Reference;

namespace Eu.EDelivery.AS4.Security.Repositories
{
    /// <summary>
    /// Repository that updates the Attachment <see cref="Reference"/>
    /// </summary>
    [Obsolete("This class is never used.")]
    public class AttachmentReferenceRepository
    {
        private readonly Reference _reference;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachmentReferenceRepository"/> class
        /// </summary>
        /// <param name="reference"></param>
        public AttachmentReferenceRepository(Reference reference)
        {
            _reference = reference;
        }

        /// <summary>
        /// Resets the reference stream position to 0.
        /// </summary>
        public void ResetReferenceStreamPosition()
        {
            FieldInfo fieldInfo = typeof(Reference).GetField(
                "m_refTarget",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo == null)
            {
                return;
            }

            var referenceStream = fieldInfo.GetValue(_reference) as Stream;            

            if (referenceStream != null)
            {
                Stream streamToWorkOn = referenceStream;

                if (referenceStream is NonCloseableStream)
                {
                    streamToWorkOn = ((NonCloseableStream) referenceStream).InnerStream;
                }
                else if (referenceStream is FilteredStream)
                {
                    streamToWorkOn = ((FilteredStream) referenceStream).Source;
                }

                if (streamToWorkOn.CanSeek && streamToWorkOn.Position != 0)
                {
                    streamToWorkOn.Position = 0;
                }               
            }            
        }

        /// <summary>
        /// Update with <paramref name="attachment"/> related information
        /// </summary>
        /// <param name="attachment"></param>
        public void UpdateWithAttachment(Attachment attachment)
        {
            SetReferenceStream(attachment);
            SetAttachmentTransformContentType(attachment);
        }

        /// <summary>
        /// Sets the stream of a SignedInfo reference.
        /// </summary>
        /// <param name="attachment"></param>
        private void SetReferenceStream(Attachment attachment)
        {
            // We need reflection to set these 2 types. They are implicitly set to Xml references, 
            // but this causes problems with cid: references, since they're not part of the original stream.
            // If performance is slow on this, we can investigate the Delegate.CreateDelegate method to speed things up, 
            // however keep in mind that the reference object changes with every call, so we can't just keep the same delegate and call that.
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo fieldInfo = typeof(Reference).GetField("m_refTargetType", bindingFlags);

            const int streamReferenceTargetType = 0;
            fieldInfo?.SetValue(this._reference, streamReferenceTargetType);

            fieldInfo = typeof(Reference).GetField("m_refTarget", bindingFlags);
            fieldInfo?.SetValue(this._reference, new NonCloseableStream(attachment.Content));
        }

        private void SetAttachmentTransformContentType(Attachment attachment)
        {
            foreach (object transform in this._reference.TransformChain)
            {
                var attachmentTransform = transform as AttachmentSignatureTransform;
                if (attachmentTransform != null)
                    attachmentTransform.ContentType = attachment.ContentType;
            }
        }
    }
}