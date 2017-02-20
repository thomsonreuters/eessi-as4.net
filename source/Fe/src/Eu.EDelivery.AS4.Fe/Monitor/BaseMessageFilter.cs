using System;
using System.Linq;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public abstract class BaseMessageFilter<TInput, TInputType, TOutput> : BaseFilter<TInput, TOutput>
        where TInputType: MessageEntity
        where TInput : BaseMessageJoined<TInputType>
    {
        public string EbmsMessageId { get; set; }
        public string EbmsRefToMessageId { get; set; }
        public string ContentType { get; set; }
        public Operation? Operation { get; set; }
        public DateTime? InsertionTimeFrom { get; set; }
        public DateTime? InsertionTimeTo { get; set; }
        public DateTime? ModificationTimeFrom { get; set; }
        public DateTime? ModificationTimeTo { get; set; }
        public MessageExchangePattern? MEP { get; set; }
        public MessageType? EbmsMessageType { get; set; }
        public ExceptionType? ExceptionType { get; set; }

        public override IQueryable<TInput> ApplyFilter(IQueryable<TInput> query)
        {
            if (Operation != null) query = query.Where(qr => qr.Message.Operation == Operation);

            if (!string.IsNullOrEmpty(EbmsMessageId))
            {
                var filter = EbmsMessageId.Replace("*", "");
                if (EbmsMessageId.StartsWith("*") && EbmsMessageId.EndsWith("*")) query = query.Where(qr => qr.Message.EbmsMessageId.Contains(filter));
                else if (EbmsMessageId.EndsWith("*")) query = query.Where(qr => qr.Message.EbmsMessageId.StartsWith(filter));
                else if (EbmsMessageId.StartsWith("*")) query = query.Where(qr => qr.Message.EbmsMessageId.EndsWith(filter));
                else query = query.Where(qr => qr.Message.EbmsMessageId == filter);
            }

            if (!string.IsNullOrEmpty(EbmsRefToMessageId))
            {
                var filter = EbmsRefToMessageId.Replace("*", "");
                if (EbmsRefToMessageId.StartsWith("*") && EbmsRefToMessageId.EndsWith("*")) query = query.Where(qr => qr.Message.EbmsRefToMessageId.Contains(filter));
                else if (EbmsRefToMessageId.EndsWith("*")) query = query.Where(qr => qr.Message.EbmsRefToMessageId.StartsWith(filter));
                else if (EbmsRefToMessageId.StartsWith("*")) query = query.Where(qr => qr.Message.EbmsRefToMessageId.EndsWith(filter));
                else query = query.Where(qr => qr.Message.EbmsRefToMessageId == filter);
            }

            if (Operation != null) query = query.Where(qr => qr.Message.Operation == Operation);

            if (InsertionTimeFrom != null && InsertionTimeTo == null) query = query.Where(qr => qr.Message.InsertionTime >= InsertionTimeFrom);
            else if (InsertionTimeFrom == null && InsertionTimeTo != null) query = query.Where(qr => qr.Message.InsertionTime <= InsertionTimeTo);
            else if (InsertionTimeFrom != null && InsertionTimeTo != null) query = query.Where(qr => qr.Message.InsertionTime >= InsertionTimeFrom && qr.Message.InsertionTime <= InsertionTimeTo);

            if (ModificationTimeFrom != null && ModificationTimeTo == null) query = query.Where(qr => qr.Message.ModificationTime >= ModificationTimeFrom);
            else if (ModificationTimeFrom == null && ModificationTimeTo != null) query = query.Where(qr => qr.Message.ModificationTime <= ModificationTimeTo);
            else if (ModificationTimeFrom != null && ModificationTimeTo != null) query = query.Where(qr => qr.Message.ModificationTime >= ModificationTimeFrom && qr.Message.ModificationTime <= ModificationTimeTo);

            if (MEP != null) query = query.Where(qr => qr.Message.MEP == MEP);
            if (EbmsMessageType != null) query = query.Where(qr => qr.Message.EbmsMessageType == EbmsMessageType);
            if (!string.IsNullOrEmpty(ContentType))
            {
                query = ContentType == "mime" ? query.Where(qr => qr.Message.ContentType.Contains("multipart/related")) : query.Where(qr => qr.Message.ContentType.StartsWith("application/soap+xml"));
            }

            return query;
        }
    }
}