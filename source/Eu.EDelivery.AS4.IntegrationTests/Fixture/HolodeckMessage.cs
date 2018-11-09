using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Eu.EDelivery.AS4.IntegrationTests.Fixture
{
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://holodeck-b2b.org/schemas/2014/06/mmd")]
    [XmlRoot(Namespace = "http://holodeck-b2b.org/schemas/2014/06/mmd", IsNullable = false)]
    public class MessageMetaData
    {
        public HolodeckCollaborationInfo CollaborationInfo { get; set; }
        public HolodeckPayloadInfo PayloadInfo { get; set; }
    }

    /// <remarks />
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://holodeck-b2b.org/schemas/2014/06/mmd")]
    public class HolodeckCollaborationInfo
    {
        public HolodeckAgreementRef AgreementRef { get; set; }

        public string ConversationId { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://holodeck-b2b.org/schemas/2014/06/mmd")]
    public class HolodeckAgreementRef
    {
        [XmlAttribute("pmode")]
        public string PMode { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://holodeck-b2b.org/schemas/2014/06/mmd")]
    public class HolodeckPayloadInfo
    {
        [XmlElement("PartInfo")]
        public HolodeckPartInfo[] PartInfo { get; set; }
    }


    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://holodeck-b2b.org/schemas/2014/06/mmd")]
    public class HolodeckPartInfo
    {
        [XmlAttribute("containment")]
        public string Containment { get; set; }

        [XmlAttribute("mimeType")]
        public string MimeType { get; set; }

        [XmlAttribute("location")]
        public string Location { get; set; }
    }
}