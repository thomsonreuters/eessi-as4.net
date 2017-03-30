using System;
using System.Linq;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public abstract class BaseExceptionFilter<TInput, TOutput> : BaseFilter<TInput, TOutput>
        where TInput: ExceptionEntity
    {
        public string EbmsRefToMessageId { get; set; }
        public DateTime? InsertionTimeFrom { get; set; }
        public DateTime? InsertionTimeTo { get; set; }
        public DateTime? ModificationTimeFrom { get; set; }
        public DateTime? ModificationTimeTo { get; set; }
        public Operation? Operation { get; set; }

        public override IQueryable<TInput> ApplyFilter(IQueryable<TInput> query)
        {
            if (!string.IsNullOrEmpty(EbmsRefToMessageId))
            {
                var filter = EbmsRefToMessageId.Replace("*", "");
                if (EbmsRefToMessageId.StartsWith("*") && EbmsRefToMessageId.EndsWith("*")) query = query.Where(qr => qr.EbmsRefToMessageId.Contains(filter));
                else if (EbmsRefToMessageId.EndsWith("*")) query = query.Where(qr => qr.EbmsRefToMessageId.StartsWith(filter));
                else if (EbmsRefToMessageId.StartsWith("*")) query = query.Where(qr => qr.EbmsRefToMessageId.EndsWith(filter));
                else query = query.Where(qr => qr.EbmsRefToMessageId == filter);
            }

            if (InsertionTimeFrom != null && InsertionTimeTo == null) query = query.Where(qr => qr.InsertionTime >= InsertionTimeFrom);
            else if (InsertionTimeFrom == null && InsertionTimeTo != null) query = query.Where(qr => qr.InsertionTime <= InsertionTimeTo);
            else if (InsertionTimeFrom != null && InsertionTimeTo != null) query = query.Where(qr => qr.InsertionTime >= InsertionTimeFrom && qr.InsertionTime <= InsertionTimeTo);

            if (ModificationTimeFrom != null && ModificationTimeTo == null) query = query.Where(qr => qr.ModificationTime >= ModificationTimeFrom);
            else if (ModificationTimeFrom == null && ModificationTimeTo != null) query = query.Where(qr => qr.ModificationTime <= ModificationTimeTo);
            else if (ModificationTimeFrom != null && ModificationTimeTo != null) query = query.Where(qr => qr.ModificationTime >= ModificationTimeFrom && qr.ModificationTime <= ModificationTimeTo);

            if (Operation != null) query = query.Where(qr => qr.Operation == Operation);
            return query;
        }
    }
}