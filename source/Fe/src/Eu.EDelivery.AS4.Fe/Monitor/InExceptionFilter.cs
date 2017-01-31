using System;
using System.Linq;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public enum Direction
    {
        In,
        Out
    }

    public class InMessageFilter : BaseFilter<InMessage, Message>
    {
        public override IQueryable<InMessage> ApplyFilter(IQueryable<InMessage> query)
        {
            
        }

        private IQueryable<InMessage> Test(IQueryable<MessageEntity> query)
        {
            
        }
    }

    public class OutMessageFilter : BaseFilter<OutMessage, Message>
    {
        public override IQueryable<OutMessage> ApplyFilter(IQueryable<OutMessage> query)
        {
            throw new NotImplementedException();
        }
    }

    public class InExceptionFilter : BaseFilter<InException, Message>
    {
        public int? Id { get; set; }
        public Operation? Operation { get; set; }
        public string OperationMethod { get; set; }
        public string EbmsRefToMessageId { get; set; }
        public string PMode { get; set; }
        public DateTime? ModificationTimeTo { get; set; }
        public DateTime? ModificationTimeFrom { get; set; }
        public DateTime? InsertionTimeTo { get; set; }
        public DateTime? InsertionTimeFrom { get; set; }
        public ExceptionType? ExceptionType { get; set; }
        public override IQueryable<InException> ApplyFilter(IQueryable<InException> query)
        {
            if (Id != null) query = query.Where(qr => qr.Id == Id);
            if (Operation != null) query = query.Where(qr => qr.Operation == Operation);
            if (!string.IsNullOrEmpty(OperationMethod)) query = query.Where(qr => qr.OperationMethod == OperationMethod);

            if (!string.IsNullOrEmpty(EbmsRefToMessageId))
            {
                var filter = EbmsRefToMessageId.Replace("*", "");
                if (EbmsRefToMessageId.StartsWith("*") && EbmsRefToMessageId.EndsWith("*")) query = query.Where(qr => qr.EbmsRefToMessageId.Contains(filter));
                else if (EbmsRefToMessageId.EndsWith("*")) query = query.Where(qr => qr.EbmsRefToMessageId.StartsWith(filter));
                else if (EbmsRefToMessageId.StartsWith("*")) query = query.Where(qr => qr.EbmsRefToMessageId.EndsWith(filter));
                else query = query.Where(qr => qr.EbmsRefToMessageId == filter);
            }

            if (!string.IsNullOrEmpty(PMode)) query = query.Where(qr => qr.PMode == PMode);

            if (ModificationTimeFrom != null && ModificationTimeTo == null) query = query.Where(qr => qr.ModificationTime >= ModificationTimeFrom);
            else if (ModificationTimeFrom == null && ModificationTimeTo != null) query = query.Where(qr => qr.ModificationTime <= ModificationTimeTo);
            else if (ModificationTimeFrom != null && ModificationTimeTo != null) query = query.Where(qr => qr.ModificationTime >= ModificationTimeFrom && qr.ModificationTime <= ModificationTimeTo);

            if (InsertionTimeFrom != null && InsertionTimeTo == null) query = query.Where(qr => qr.InsertionTime >= InsertionTimeFrom);
            else if (InsertionTimeFrom == null && InsertionTimeTo != null) query = query.Where(qr => qr.InsertionTime <= InsertionTimeTo);
            else if (InsertionTimeFrom != null && InsertionTimeTo != null) query = query.Where(qr => qr.InsertionTime >= InsertionTimeFrom && qr.InsertionTime <= InsertionTimeTo);

            if (ExceptionType != null) query = query.Where(qr => qr.ExceptionType == ExceptionType);

            return query;
        }
    }
}