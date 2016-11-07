using System;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Text.RegularExpressions;
using System.Xml;

namespace Eu.EDelivery.AS4.Security.Transforms
{
    public class AttachmentCiphertextTransform : Transform
    {
        public const string Url = "http://docs.oasis-open.org/wss/oasis-wss-SwAProfile-1.1#Attachment-Ciphertext-Transform";

        private Stream _inputStream;

        public override Type[] InputTypes { get; } = {typeof(Stream)};
        public override Type[] OutputTypes { get; } = {typeof(Stream)};

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachmentCiphertextTransform"/> class
        /// </summary>
        public AttachmentCiphertextTransform()
        {
            this.Algorithm = Url;
        }

        public AttachmentCiphertextTransform(string contentType)
        {
            this.Algorithm = Url;
        }

        public override void LoadInnerXml(XmlNodeList nodeList) {}

        protected override XmlNodeList GetInnerXml()
        {
            return null;
        }

        public override void LoadInput(object obj)
        {
            _inputStream = obj as Stream;
        }

        public override object GetOutput()
        {
            return this._inputStream;
        }

        public override object GetOutput(Type type)
        {
            return this._inputStream;
        }
    }
}