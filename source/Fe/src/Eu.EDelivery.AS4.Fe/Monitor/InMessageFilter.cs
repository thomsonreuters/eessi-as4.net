using System.Linq;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public class InMessageFilter : BaseMessageFilter<InMessageJoined, InMessage, Message>
    {
        public InStatus? Status { get; set; }

        public override IQueryable<InMessageJoined> ApplyFilter(IQueryable<InMessageJoined> query)
        {
            query = base.ApplyFilter(query);
            if (Status != null) query = query.Where(qr => qr.Message.Status == Status);
            return query;
        }
    }
}