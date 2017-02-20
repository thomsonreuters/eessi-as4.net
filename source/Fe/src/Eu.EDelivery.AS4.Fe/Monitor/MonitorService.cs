using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Fe.Pmodes;
using Microsoft.EntityFrameworkCore;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public class MonitorService : IMonitorService
    {
        private readonly DatastoreContext context;
        private readonly IAs4PmodeSource pmodeSource;

        public MonitorService(DatastoreContext context, IAs4PmodeSource pmodeSource)
        {
            this.context = context;
            this.pmodeSource = pmodeSource;
        }

        public async Task<MessageResult<ExceptionMessage>> GetInExceptions(string inMessageId)
        {
            var data = await context
                .InExceptions
                .Where(msg => msg.EbmsRefToMessageId == inMessageId)
                .ProjectTo<ExceptionMessage>()
                .ToListAsync();
            return new MessageResult<ExceptionMessage>
            {
                CurrentPage = 0,
                Messages = data,
                Page = 0,
                Pages = 0,
                Total = data.Count
            };
        }

        public async Task<MessageResult<ExceptionMessage>> GetOutExceptions(string outMessageId)
        {
            var data = await context
                .OutExceptions
                .Where(msg => msg.EbmsRefToMessageId == outMessageId)
                .ProjectTo<ExceptionMessage>()
                .ToListAsync();
            return new MessageResult<ExceptionMessage>
            {
                CurrentPage = 0,
                Messages = data,
                Page = 0,
                Pages = 0,
                Total = data.Count
            };
        }

        public async Task<MessageResult<Message>> GetInMessages(InMessageFilter filter)
        {
            var messages = context
                .InMessages
                .OrderByDescending(msg => msg.InsertionTime)
                .Select(x => new InMessageJoined
                {
                    Message = x
                });

            var result = ConvertPmodeXmlToNumbers(await filter.ToResult(messages));
            var messageIDs = result
                .Messages
                .Select(msg => msg.EbmsRefToMessageId)
                .Distinct();

            var exceptions = context
                .InExceptions
                .Select(ex => ex.EbmsRefToMessageId)
                .Where(ex => messageIDs.Contains(ex))
                .Distinct()
                .ToList();

            result.Messages = result.Messages.Select(msg =>
            {
                msg.HasExceptions = exceptions.Any(ex => ex == msg.EbmsRefToMessageId);
                return msg;
            });

            return result;
        }

        public async Task<MessageResult<Message>> GetOutMessages(OutMessageFilter filter)
        {
            var messages = context
                .OutMessages
                .OrderByDescending(msg => msg.InsertionTime)
                .Select(x => new OutMessageJoined
                {
                    Message = x
                });

            var result = ConvertPmodeXmlToNumbers(await filter.ToResult(messages));
            var messageIDs = result
                .Messages
                .Select(msg => msg.EbmsRefToMessageId)
                .Distinct();

            var exceptions = context
                .OutExceptions
                .Select(ex => ex.EbmsRefToMessageId)
                .Where(ex => messageIDs.Contains(ex))
                .Distinct()
                .ToList();

            result.Messages = result.Messages.Select(msg =>
            {
                msg.HasExceptions = exceptions.Any(ex => ex == msg.EbmsRefToMessageId);
                return msg;
            });

            return result;
        }

        public string GetPmodeNumber(string pmode)
        {
            return string.IsNullOrEmpty(pmode) ? string.Empty : pmodeSource.GetPmodeNumber(pmode);
        }

        private MessageResult<Message> ConvertPmodeXmlToNumbers(MessageResult<Message> result)
        {
            foreach (var message in result.Messages)
                message.PMode = GetPmodeNumber(message.PMode);

            return result;
        }
    }
}