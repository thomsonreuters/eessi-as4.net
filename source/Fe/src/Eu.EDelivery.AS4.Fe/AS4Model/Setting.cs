using System.Xml.Serialization;

namespace Eu.EDelivery.AS4.Fe.AS4Model
{
    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    [XmlRoot(Namespace = "eu:edelivery:as4", IsNullable = false)]
    public class Settings
    {
        public string IdFormat { get; set; }
        public SettingsDatabase Database { get; set; }
        public CertificateStore CertificateStore { get; set; }
        public CustomSettings CustomSettings { get; set; }
        public SettingsAgents Agents { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class CertificateStore
    {
        public string StoreName { get; set; }
        public Repository Repository { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class Repository
    {
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class CustomSettings
    {
        [XmlElement("Setting")]
        public Setting[] Setting { get; set; }
    }

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
    }

    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class SettingsDatabase
    {
        public string Provider { get; set; }
        public string ConnectionString { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class SettingsAgent
    {
        [XmlElement("Receiver")]
        public Receiver Receiver { get; set; }
        [XmlElement("Transformer")]
        public Transformer Transformer { get; set; }
        [XmlElement("Steps")]
        public Steps Steps { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class Steps
    {
        [XmlAttribute(AttributeName = "decorator")]
        public string Decorator { get; set; }
        [XmlElement("Step")]
        public Step[] Step { get; set; }
    }

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

    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class ReceiveAgent
    {
        [XmlElement("Receiver")]
        public Receiver Receiver { get; set; }
        [XmlElement("Transformer")]
        public Transformer Transformer { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class Receiver
    {
        [XmlElement("Setting")]
        public Setting[] Setting { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "eu:edelivery:as4")]
    public class Setting
    {
        [XmlAttribute(AttributeName = "key")]
        public string Key { get; set; }
        [XmlText]
        public string Value { get; set; }
    }

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
