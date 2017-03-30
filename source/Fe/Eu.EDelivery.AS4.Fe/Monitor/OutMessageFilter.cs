using System.Linq;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public class OutMessageFilter : BaseMessageFilter<OutMessageJoined, OutMessage, Message>
    {
        public OutStatus? Status { get; set; }

        public override IQueryable<OutMessageJoined> ApplyFilter(IQueryable<OutMessageJoined> query)
        {
            query = base.ApplyFilter(query);
            if (Status != null) query = query.Where(qr => qr.Message.Status == Status);
            return query;
        }
    }
}