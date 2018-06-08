using System;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eu.EDelivery.AS4.Model.Internal
{
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    [XmlRoot(Namespace = "eu:edelivery:as4", IsNullable = false)]
    public class Settings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings()
        {
            FeInProcess = true;
            PayloadServiceInProcess = true;
        }

        public string IdFormat { get; set; }

        public bool FeInProcess { get; set; }

        public bool PayloadServiceInProcess { get; set; }

        public string RetentionPeriod { get; set; }

        public SettingsRetryReliability RetryReliability { get; set; }

        public SettingsDatabase Database { get; set; }

        public CertificateStore CertificateStore { get; set; }

        public CustomSettings CustomSettings { get; set; }

        public SettingsAgents Agents { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class SettingsRetryReliability
    {
        public string PollingInterval { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class CertificateStore
    {
        public string StoreName { get; set; }

        public Repository Repository { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class Repository
    {
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class CustomSettings
    {
        [XmlElement("Setting")]
        public Setting[] Setting { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class SettingsAgents
    {
        [XmlElement("SubmitAgent", IsNullable = false)]
        public AgentSettings[] SubmitAgents { get; set; }

        [XmlElement("OutboundProcessingAgent", IsNullable = false)]
        public AgentSettings[] OutboundProcessingAgents { get; set; }

        [XmlElement("SendAgent", IsNullable = false)]
        public AgentSettings[] SendAgents { get; set; }

        [XmlElement("ReceiveAgent", IsNullable = false)]
        public AgentSettings[] ReceiveAgents { get; set; }

        [XmlElement("DeliverAgent", IsNullable = false)]
        public AgentSettings[] DeliverAgents { get; set; }
                
        [XmlElement("NotifyAgent")]
        public AgentSettings[] NotifyAgents { get; set; }

        [XmlElement("ForwardAgent", IsNullable = false)]
        public AgentSettings[] ForwardAgents { get; set; }

        [XmlElement("ReceptionAwarenessAgent", IsNullable = false)]
        public AgentSettings ReceptionAwarenessAgent { get; set; }

        [XmlElement("PullReceiveAgent", IsNullable = false)]
        public AgentSettings[] PullReceiveAgents { get; set; }

        [XmlElement("PullSendAgent", IsNullable = false)]
        public AgentSettings[] PullSendAgents { get; set; }

        [XmlElement("MinderSubmitReceiveAgent", IsNullable = true)]
        public SettingsMinderAgent[] MinderTestAgents { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class SettingsMinderAgent
    {
        /// <summary>
        /// Gets or sets a value indicating whether the Agent should be enabled or not.
        /// </summary>
        [XmlAttribute("Enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the URL at which the Agent should listen to.
        /// </summary>
        [XmlAttribute("Url")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not logging of received messages is enabled.
        /// </summary>
        [XmlAttribute("UseLogging")]
        public bool UseLogging { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Transformer that should be used to transform a received message.
        /// </summary>
        [XmlElement("Transformer")]
        public Transformer Transformer { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class SettingsDatabase
    {
        public string Provider { get; set; }

        public string ConnectionString { get; set; }

        public string InMessageStoreLocation { get; set; }

        public string OutMessageStoreLocation { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class AgentSettings
    {
        [XmlElement("Receiver")]
        public Receiver Receiver { get; set; }

        [XmlElement("Transformer")]
        public Transformer Transformer { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlElement("StepConfiguration")]
        public StepConfiguration StepConfiguration { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class StepConfiguration
    {
        [XmlArray("NormalPipeline")]
        [XmlArrayItem("Step")]
        public Step[] NormalPipeline { get; set; }

        [XmlArray("ErrorPipeline")]
        [XmlArrayItem("Step")]
        public Step[] ErrorPipeline { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class Step
    {
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlElement("Setting")]
        public Setting[] Setting { get; set; }
    }

    /// <summary>
    /// Defines the configuration of a ConditionalStep
    /// </summary>
    /// <remarks>This class is not serializable.  Only used programmatically for conformonce-testing.</remarks>
    public class ConditionalStepConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalStepConfig" /> class.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="thenSteps">The then steps.</param>
        /// <param name="elseSteps">The else steps.</param>
        public ConditionalStepConfig(Func<MessagingContext, bool> condition, Step[] thenSteps, Step[] elseSteps)
        {
            Condition = condition;
            ThenSteps = thenSteps;
            ElseSteps = elseSteps;
        }

        public Func<MessagingContext, bool> Condition { get; }

        public Step[] ThenSteps { get; }

        public Step[] ElseSteps { get; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class ReceiveAgent
    {
        [XmlElement("Receiver")]
        public Receiver Receiver { get; set; }

        [XmlElement("Transformer")]
        public Transformer Transformer { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class Receiver
    {
        [XmlElement("Setting")]
        public Setting[] Setting { get; set; }

        [XmlText]
        public string[] Text { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
    }

    public class XmlAttributeArrayConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(XmlAttribute[]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            var array = JArray.Load(reader);
            var doc = new XmlDocument();
            var test = array.Select(x => (JObject)x).SelectMany(x => x.Properties()).ToDictionary(pair => pair.Name, pair => pair.Value.Value<string>());
            var result = test.Select(x =>
            {
                var a = doc.CreateAttribute(x.Key);
                a.Value = x.Value;
                return a;
            }).ToArray();
            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var test = (XmlAttribute[])value;

            writer.WriteStartArray();
            foreach (var attribute in test)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(attribute.Name);
                writer.WriteValue(attribute.Value);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            //JArray.FromObject(value).WriteTo(writer);
            //throw new NotImplementedException();
        }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class Setting
    {
        [XmlAttribute(AttributeName = "key")]
        public string Key { get; set; }

        [XmlAnyAttribute]
        [JsonConverter(typeof(XmlAttributeArrayConverter))]
        public XmlAttribute[] Attributes { get; set; }

        [XmlText]
        public string Value { get; set; }

        /// <summary>
        /// Prevents a default instance of the <see cref="Setting"/> class from being created. 
        /// </summary>
        private Setting() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Setting"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public Setting(string key, string value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Gets the attribute for a given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name for which an Attribute is retrieved.</param>
        /// <returns></returns>
        public XmlAttribute this[string name]
        {
            get { return Attributes?.FirstOrDefault(a => a.LocalName.Equals(name)); }
        }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class Transformer
    {
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlElement("Setting")]
        public Setting[] Setting { get; set; }
    }
}