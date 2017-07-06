using System;
using System.Linq;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Fe.Monitor.Model
{
    public class MessageFilter : BaseFilter<MessageEntity, Message>
    {
        public Direction[] Direction { get; set; } = new[] { Model.Direction.Inbound, Model.Direction.Outbound };
        public string EbmsMessageId { get; set; }
        public string EbmsRefToMessageId { get; set; }
        public string[] ContentType { get; set; }
        public Operation[] Operation { get; set; }
        public DateTime? InsertionTimeFrom { get; set; }
        public DateTime? InsertionTimeTo { get; set; }
        public DateTime? ModificationTimeFrom { get; set; }
        public DateTime? ModificationTimeTo { get; set; }
        public MessageExchangePattern[] MEP { get; set; }
        public MessageType[] EbmsMessageType { get; set; }
        public StatusEnum[] Status { get; set; }
        public string FromParty { get; set; }
        public string ToParty { get; set; }
        public bool ShowDuplicates { get; set; }
        public bool ShowTests { get; set; }
        public IQueryable<TEntity> ApplyFilter<TEntity>(IQueryable<TEntity> query)
            where TEntity: MessageEntity
        {
            if (!string.IsNullOrEmpty(EbmsMessageId))
            {
                var filter = EbmsMessageId.Replace("*", "");
                if (EbmsMessageId.StartsWith("*") && EbmsMessageId.EndsWith("*")) query = query.Where(qr => qr.EbmsMessageId.Contains(filter));
                else if (EbmsMessageId.EndsWith("*")) query = query.Where(qr => qr.EbmsMessageId.StartsWith(filter));
                else if (EbmsMessageId.StartsWith("*")) query = query.Where(qr => qr.EbmsMessageId.EndsWith(filter));
                else query = query.Where(qr => qr.EbmsMessageId == filter);
            }

            if (!string.IsNullOrEmpty(EbmsRefToMessageId))
            {
                var filter = EbmsRefToMessageId.Replace("*", "");
                if (EbmsRefToMessageId.StartsWith("*") && EbmsRefToMessageId.EndsWith("*")) query = query.Where(qr => qr.EbmsRefToMessageId.Contains(filter));
                else if (EbmsRefToMessageId.EndsWith("*")) query = query.Where(qr => qr.EbmsRefToMessageId.StartsWith(filter));
                else if (EbmsRefToMessageId.StartsWith("*")) query = query.Where(qr => qr.EbmsRefToMessageId.EndsWith(filter));
                else query = query.Where(qr => qr.EbmsRefToMessageId == filter);
            }

            if (Operation != null)
            {
                var operations = Operation.Select(op => op.ToString());
                query = query.Where(qr => operations.Contains(qr.OperationString));
            }

            if (InsertionTimeFrom != null && InsertionTimeTo == null) query = query.Where(qr => qr.InsertionTime >= InsertionTimeFrom);
            else if (InsertionTimeFrom == null && InsertionTimeTo != null) query = query.Where(qr => qr.InsertionTime <= InsertionTimeTo);
            else if (InsertionTimeFrom != null && InsertionTimeTo != null) query = query.Where(qr => qr.InsertionTime >= InsertionTimeFrom && qr.InsertionTime <= InsertionTimeTo);

            if (ModificationTimeFrom != null && ModificationTimeTo == null) query = query.Where(qr => qr.ModificationTime >= ModificationTimeFrom);
            else if (ModificationTimeFrom == null && ModificationTimeTo != null) query = query.Where(qr => qr.ModificationTime <= ModificationTimeTo);
            else if (ModificationTimeFrom != null && ModificationTimeTo != null) query = query.Where(qr => qr.ModificationTime >= ModificationTimeFrom && qr.ModificationTime <= ModificationTimeTo);

            if (MEP != null)
            {
                var mepStrings = MEP.Select(mep => mep.ToString());
                query = query.Where(qr => mepStrings.Contains(qr.MEPString));
            }
            if (EbmsMessageType != null)
            {
                var messageTypeStrings = EbmsMessageType.Select(type => type.ToString());
                query = query.Where(qr => messageTypeStrings.Contains(qr.EbmsMessageTypeString));
            }
            if (ContentType != null)
            {
                var types = ContentType.Select(x => x == "mime" ? "multipart/related" : "application/soap+xml").ToList();
                query = query.Where(qr => types.Any(data => qr.ContentType.StartsWith(data)));
            }
            if (Status != null && Status.Any())
            {
                var statusStrings = Status.Select(status => status.ToString()).ToList();
                query = query.Where(qr => statusStrings.Contains(qr.StatusString));
            }

            if (!string.IsNullOrEmpty(FromParty)) query = query.Where(x => x.FromParty == FromParty);
            if (!string.IsNullOrEmpty(ToParty)) query = query.Where(x => x.ToParty == ToParty);
            if (!ShowDuplicates) query = query.Where(x => !x.IsDuplicate);
            if (!ShowTests) query = query.Where(x => !x.IsTest);

            return query;
        }

        public IQueryable<Message> ApplyStatusFilter(IQueryable<Message> query)
        {
            if (Status == null || !Status.Any()) return query;
            var statusFilter = Status.Select(status => status.ToString()).ToList();
            return query.Where(qr => statusFilter.Contains(qr.Status));
        }
    }
}