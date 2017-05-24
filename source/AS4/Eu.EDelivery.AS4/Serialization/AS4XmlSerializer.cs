using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Serialization
{
    /// <summary>
    /// <see cref="AS4Message" /> Serializer to Xml.
    /// </summary>
    public static class AS4XmlSerializer
    {
        private static readonly IDictionary<Type, XmlSerializer> Serializers =
            new ConcurrentDictionary<Type, XmlSerializer>();

        /// <summary>
        /// Serializer a given data model to a Xml Stream.
        /// </summary>
        /// <typeparam name="T">Type of the class to which the data model must be serialized.</typeparam>
        /// <param name="data">Given data instance to serialize.</param>
        /// <returns></returns>
        public static Stream ToStream<T>(T data)
        {
            string xml = ToString(data);
            return new MemoryStream(Encoding.UTF8.GetBytes(xml));
        }

        /// <summary>
        /// Serialize Model into Xml String
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static async Task<string> ToStringAsync<T>(T data)
        {
            return await Task.Run(() => ToString(data));
        }

        /// <summary>
        /// Serialize Model into Xml String
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ToString<T>(T data)
        {
            var stringBuilder = new StringBuilder();
            using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, DefaultXmlWriterSettings))
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(xmlWriter, data);

                return stringBuilder.ToString();
            }
        }

        /// <summary>
        /// To the document.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static XmlDocument ToDocument(InternalMessage message, CancellationToken cancellationToken)
        {
            using (var memoryStream = new MemoryStream())
            {
                var provider = SerializerProvider.Default;

                ISerializer serializer = provider.Get(Constants.ContentTypes.Soap);
                serializer.Serialize(message.AS4Message, memoryStream, cancellationToken);

                return LoadEnvelopeToDocument(memoryStream);
            }
        }

        private static readonly XmlWriterSettings DefaultXmlWriterSettings = new XmlWriterSettings
        {
            CloseOutput = false,
            Encoding = new UTF8Encoding(false)
        };

        private static XmlDocument LoadEnvelopeToDocument(Stream envelopeStream)
        {
            envelopeStream.Position = 0;
            var envelopeXmlDocument = new XmlDocument() {PreserveWhitespace = true};

            envelopeXmlDocument.Load(envelopeStream);

            return envelopeXmlDocument;
        }

        /// <summary>
        /// Deserialize a Xml stream to a Model.
        /// </summary>
        /// <typeparam name="T">Type to which the given stream must be deserialized.</typeparam>
        /// <param name="stream">Stream containing the Xml.</param>
        /// <returns></returns>
        public static T FromStream<T>(Stream stream) where T : class
        {
            using (var streamReader = new StreamReader(stream))
            {
                stream.Position = 0;

                string xml = streamReader.ReadToEnd();
                return FromString<T>(xml);
            }
        }

        /// <summary>
        /// Deserialize Xml String to Model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static T FromString<T>(string xml) where T : class
        {
            if (xml == null)
            {
                return null;
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(xml)))
            {
                var serializer = new XmlSerializer(typeof(T));
                if (serializer.CanDeserialize(reader))
                {
                    return serializer.Deserialize(reader) as T;
                }

                return null;
            }
        }

        /// <summary>
        /// Deserialize to Model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T FromReader<T>(XmlReader reader) where T : class
        {
            XmlSerializer serializer = GetSerializerForType(typeof(T));
            return serializer.Deserialize(reader) as T;
        }

        private static XmlSerializer GetSerializerForType(Type type)
        {
            if (!Serializers.ContainsKey(type)) Serializers[type] = new XmlSerializer(type);
            return Serializers[type];
        }
    }
}