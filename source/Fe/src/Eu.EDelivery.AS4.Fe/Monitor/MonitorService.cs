using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public class MonitorService : IMonitorService
    {
        private readonly DatastoreContext context;

        public MonitorService(DatastoreContext context)
        {
            this.context = context;
        }

        public Task<MessageResult<InException>> GetExceptions(InExceptionFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<MessageResult<Message>> GetInMessages(InMessageFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task<MessageResult<Message>> GetOutMessages(OutMessageFilter filter)
        {
            throw new NotImplementedException();
        }
    }

    public class Message
    {
        public string Name { get; set; }
    }

    public class InMessage : Message
    {
        public string Test { get; set; }
    }

    public class OutMessage : Message
    {
        public string AndereTest { get; set; }
    }
}