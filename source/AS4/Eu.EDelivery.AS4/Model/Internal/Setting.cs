using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Eu.EDelivery.AS4.Model.Internal
{
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    [XmlRoot(Namespace = "eu:edelivery:as4", IsNullable = false)]
    public class Settings
    {
        public string IdFormat { get; set; }

        public SettingsDatabase Database { get; set; }

        public CertificateStore CertificateStore { get; set; }

        public CustomSettings CustomSettings { get; set; }

        public SettingsAgents Agents { get; set; }

        public bool FeInProcess { get; set; }
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
        public SettingsAgent[] SubmitAgents { get; set; }

        [XmlElement("ReceiveAgent", IsNullable = false)]
        public SettingsAgent[] ReceiveAgents { get; set; }

        [XmlElement("SendAgent", IsNullable = false)]
        public SettingsAgent[] SendAgents { get; set; }

        [XmlElement("DeliverAgent", IsNullable = false)]
        public SettingsAgent[] DeliverAgents { get; set; }

        [XmlElement("NotifyAgent", IsNullable = false)]
        public SettingsAgent[] NotifyAgents { get; set; }

        [XmlElement("ReceptionAwarenessAgent", IsNullable = false)]
        public SettingsAgent ReceptionAwarenessAgent { get; set; }

        [XmlElement("PullReceiveAgent", IsNullable = false)]
        public SettingsAgent[] PullReceiveAgents { get; set; }

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
        /// Gets or sets a value indicating wheter the URL at which the Agent should listen to.
        /// </summary>
        [XmlAttribute("Url")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating wheter the Transformer that should be used to transform a received message.
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
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class SettingsAgent
    {
        [XmlElement("Receiver")]
        public Receiver Receiver { get; set; }

        [XmlElement("Transformer")]
        public Transformer Transformer { get; set; }

        [XmlElement("Steps")]
        public Steps Steps { get; set; }

        // TODO: define decorator strategy for the .xml document
        [XmlElement("Decorator")]
        public Decorator Decorator { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class Steps
    {
        [XmlAttribute(AttributeName = "decorator")]
        public string Decorator { get; set; }

        [XmlElement("Step")]
        public Step[] Step { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class Step
    {
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "undecorated")]
        public bool UnDecorated { get; set; }

        [XmlElement("Setting")]
        public Setting[] Setting { get; set; }
    }

    /// <summary>
    /// Defines the configuration of a ConditionalStep
    /// </summary>
    /// <remarks>This class is not serializable.  Only used programmatically for conformonce-testing.</remarks>
    public class ConditionalStepConfig
    {
        public ConditionalStepConfig(Func<InternalMessage, bool> condition, Steps thenStepConfig, Steps elseStepConfig)
        {
            Condition = condition;
            ThenStepConfig = thenStepConfig;
            ElseStepConfig = elseStepConfig;
        }

        public Func<InternalMessage, bool> Condition { get; }

        public Steps ThenStepConfig { get; }

        public Steps ElseStepConfig { get; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class Decorator
    {
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlElement("Steps")]
        public Steps Steps { get; set; }
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

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class Setting
    {
        [XmlAttribute(AttributeName = "key")]
        public string Key { get; set; }

        [XmlAnyAttribute]
        public XmlAttribute[] Attributes { get; set; }

        [XmlText]
        public string Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Setting"/> class.
        /// </summary>
        public Setting() {}

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
    }

    public enum AgentType
    {
        Submit,
        Receive,
        Sent,
        Deliver,
        Notify
    }
}