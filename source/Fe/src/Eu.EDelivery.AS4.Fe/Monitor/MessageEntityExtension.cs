using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public static class MessageEntityExtension
    {
        public static string SimplifyContentType(this MessageEntity entity)
        {
            return string.IsNullOrEmpty(entity?.ContentType) ? null : entity.ContentType.Contains("multipart/related") ? "Mime" : "Xml";
        }
    }
}