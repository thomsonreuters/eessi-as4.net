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
        /// Serialize a given data Model to a Xml Stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static async Task<Stream> ToStreamAsync<T>(T data)
        {
            return await Task.Run(() =>
            {
                string xml = ToString(data);
                return (Stream) new MemoryStream(Encoding.UTF8.GetBytes(xml));
            });
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
        public static XmlDocument ToSoapEnvelopeDocument(MessagingContext message, CancellationToken cancellationToken)
        {
            return SerializeToSoapEnvelope(
                message.AS4Message,
                cancellationToken,
                memoryStream => LoadEnvelopeToDocument(memoryStream));
        }

        private static readonly XmlWriterSettings DefaultXmlWriterSettings = new XmlWriterSettings
        {
            CloseOutput = false,
            Encoding = new UTF8Encoding(false)
        };

        private static XmlDocument LoadEnvelopeToDocument(Stream envelopeStream)
        {
            envelopeStream.Position = 0;
            var envelopeXmlDocument = new XmlDocument {PreserveWhitespace = true};

            envelopeXmlDocument.Load(envelopeStream);

            return envelopeXmlDocument;
        }

        /// <summary>
        /// Tries to XML bytes asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static async Task<byte[]> TryToXmlBytesAsync<T>(T data)
        {
            try
            {
                string xml = await ToStringAsync(data);
                return Encoding.UTF8.GetBytes(xml);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return new byte[0];
            }
        }

        /// <summary>
        /// To the SOAP envelope bytes.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static async Task<byte[]> ToSoapEnvelopeBytesAsync(AS4Message message)
        {
            return await Task.Run(() => SerializeToSoapEnvelope(message, CancellationToken.None, s => s.ToArray()));
        }

        private static T SerializeToSoapEnvelope<T>(
            AS4Message message,
            CancellationToken cancellation,
            Func<MemoryStream, T> handling)
        {
            using (var messageStream = new MemoryStream())
            {
                var serializer = new SoapEnvelopeSerializer();
                serializer.Serialize(message, messageStream, cancellation);

                return handling(messageStream);
            }
        }

        /// <summary>
        /// Deserialize a Xml stream to a Model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static async Task<T> FromStreamAsync<T>(Stream stream) where T : class
        {
            return await Task.Run(() =>
            {
                using (var streamReader = new StreamReader(stream))
                {
                    stream.Position = 0;

                    string xml = streamReader.ReadToEnd();
                    return FromString<T>(xml);
                }
            });
        }

        /// <summary>
        /// Deserialize Xml String to Model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml">The XML.</param>
        /// <returns></returns>
        public static async Task<T> FromStringAsync<T>(string xml) where T : class
        {
            return await Task.Run(() => FromString<T>(xml));
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
        /// Deserialize to a given type from a given reader.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        public static async Task<T> FromReaderAsync<T>(XmlReader reader) where T : class
        {
            return await Task.Run(() =>
            {
                XmlSerializer serializer = GetSerializerForType(typeof(T));
                return serializer.Deserialize(reader) as T;
            });
        }

        private static XmlSerializer GetSerializerForType(Type type)
        {
            if (!Serializers.ContainsKey(type)) Serializers[type] = new XmlSerializer(type);
            return Serializers[type];
        }
    }
}