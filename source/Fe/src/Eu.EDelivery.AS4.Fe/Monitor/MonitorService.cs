using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnsureThat;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Fe.Pmodes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Monitor.Model;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public class MonitorService : IMonitorService
    {
        private readonly DatastoreContext context;
        private readonly IAs4PmodeSource pmodeSource;
        private readonly IMapper mapper;

        public MonitorService(DatastoreContext context, IAs4PmodeSource pmodeSource, IMapper mapper)
        {
            this.context = context;
            this.pmodeSource = pmodeSource;
            this.mapper = mapper;
        }

        public async Task<MessageResult<ExceptionMessage>> GetExceptions(ExceptionFilter filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter), "Filter must be supplied");
            if (filter.Direction == null) throw new ArgumentNullException(nameof(filter.Direction), "Direction cannot be null");
            var inExceptions = filter.Direction.Contains(Direction.Inbound) ? filter.ApplyFilter(context.InExceptions).ProjectTo<ExceptionMessage>() : null;
            var outExceptions = filter.Direction.Contains(Direction.Outbound) ? filter.ApplyFilter(context.OutExceptions).ProjectTo<ExceptionMessage>() : null;

            IQueryable<ExceptionMessage> result = null;
            if (inExceptions != null && outExceptions != null) result = inExceptions.Concat(outExceptions);
            else if (inExceptions != null) result = inExceptions;
            else if (outExceptions != null) result = outExceptions;

            var returnValue = ConvertPmodeXmlToNumbers(await filter.ToResult(result.OrderByDescending(msg => msg.InsertionTime)));

            return returnValue;
        }

        public string GetPmodeNumber(string pmode)
        {
            return string.IsNullOrEmpty(pmode) ? string.Empty : pmodeSource.GetPmodeNumber(pmode);
        }

        public async Task<MessageResult<Message>> GetMessages(MessageFilter filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter), "Filter cannot be null");
            if (filter.Direction == null) throw new ArgumentNullException(nameof(filter.Direction), "Direction filter cannot be empty");

            IQueryable<InMessage> inMessageQuery = context.InMessages;
            IQueryable<OutMessage> outMessageQuery = context.OutMessages;

            var inMessages = filter.Direction.Contains(Direction.Inbound) ? filter.ApplyFilter(inMessageQuery).ProjectTo<Message>() : null;
            var outMessages = filter.Direction.Contains(Direction.Outbound) ? filter.ApplyFilter(outMessageQuery).ProjectTo<Message>() : null;

            IQueryable<Message> result = null;

            if (inMessages != null && outMessages != null) result = inMessages.Concat(outMessages);
            else if (inMessages != null) result = inMessages;
            else if (outMessages != null) result = outMessages;
            if (result == null) throw new BusinessException("No messages found");

            var returnValue = ConvertPmodeXmlToNumbers(await filter.ToResult(filter.ApplyStatusFilter(result).OrderByDescending(msg => msg.InsertionTime)));
            UpdateHasExceptions(returnValue, await GetExceptionIds(returnValue));

            return returnValue;
        }

        public async Task<MessageResult<Message>> GetRelatedMessages(Direction direction, string messageId)
        {
            EnsureArg.IsNotNullOrEmpty(messageId);

            var refToMessageId = direction == Direction.Inbound ?
                context.InMessages.Where(message => message.EbmsMessageId == messageId).Select(message => message.EbmsRefToMessageId).FirstOrDefault()
                : context.OutMessages.Where(message => message.EbmsMessageId == messageId).Select(message => message.EbmsRefToMessageId).FirstOrDefault();

            var resultTest = new List<IQueryable<Message>>();

            if (!string.IsNullOrEmpty(refToMessageId))
            {
                resultTest.Add(context
                    .InMessages
                    .Where(message => message.EbmsMessageId == refToMessageId)
                    .ProjectTo<Message>());

                resultTest.Add(context
                    .OutMessages
                    .Where(message => message.EbmsMessageId == refToMessageId)
                    .ProjectTo<Message>());
            }

            if (!string.IsNullOrEmpty(messageId))
            {
                resultTest.Add(context
                    .InMessages
                    .Where(message => message.EbmsRefToMessageId == messageId)
                    .ProjectTo<Message>());

                resultTest.Add(context
                    .OutMessages
                    .Where(message => message.EbmsRefToMessageId == messageId)
                    .ProjectTo<Message>());
            }

            var result = resultTest.FirstOrDefault();
            foreach (var query in resultTest.Skip(1))
            {
                result = result.Union(query);
            }
            return ConvertPmodeXmlToNumbers(new MessageResult<Message>
            {
                Messages = await result.ToListAsync(),
                Total = await result.CountAsync(),
                Page = 0,
                Pages = 0,
                CurrentPage = 0
            });
        }

        private static void UpdateHasExceptions(MessageResult<Message> returnValue, List<string> exceptionIds)
        {
            returnValue.Messages = returnValue.Messages.Select(x =>
            {
                x.HasExceptions = exceptionIds.Any(ex => ex == x.EbmsRefToMessageId);
                return x;
            });
        }

        private async Task<List<string>> GetExceptionIds(MessageResult<Message> returnValue)
        {
            var ids = returnValue.Messages.Select(msg => msg.EbmsRefToMessageId).ToList();

            var inExceptions = context.InExceptions.Where(ex => ids.Contains(ex.EbmsRefToMessageId)).Select(ex => ex.EbmsRefToMessageId);
            var outExceptions = context.OutExceptions.Where(ex => ids.Contains(ex.EbmsRefToMessageId)).Select(ex => ex.EbmsRefToMessageId);

            return await inExceptions.Union(outExceptions).ToListAsync();
        }

        private MessageResult<Message> ConvertPmodeXmlToNumbers(MessageResult<Message> result)
        {
            foreach (var message in result.Messages) message.PMode = GetPmodeNumber(message.PMode);
            return result;
        }

        private IEnumerable<Message> ConvertPmodeXmlToNumbers(IEnumerable<Message> result)
        {
            foreach (var message in result) message.PMode = GetPmodeNumber(message.PMode);
            return result;
        }

        private MessageResult<ExceptionMessage> ConvertPmodeXmlToNumbers(MessageResult<ExceptionMessage> result)
        {
            foreach (var message in result.Messages) message.PMode = GetPmodeNumber(message.PMode);
            return result;
        }

        public async Task<byte[]> DownloadMessageBody(Direction direction, string messageId)
        {
            if (string.IsNullOrEmpty(messageId)) throw new ArgumentNullException(nameof(messageId), "messageId parameter cannot be null");
            if (direction == Direction.Inbound)
            {
                return await context.InMessages
                    .Where(msg => msg.EbmsMessageId == messageId)
                    .Select(msg => msg.MessageBody)
                    .FirstOrDefaultAsync();
            }

            return await context.OutMessages
                .Where(msg => msg.EbmsMessageId == messageId)
                .Select(msg => msg.MessageBody)
                .FirstOrDefaultAsync();
        }

        public async Task<byte[]> DownloadExceptionBody(Direction direction, string messageId)
        {
            if (string.IsNullOrEmpty(messageId)) throw new ArgumentNullException(nameof(messageId), "messageId parameter cannot be null");
            if (direction == Direction.Inbound)
            {
                return await context.InExceptions
                    .Where(msg => msg.EbmsRefToMessageId == messageId)
                    .Select(msg => msg.MessageBody)
                    .FirstOrDefaultAsync();
            }

            return await context.OutExceptions
                .Where(msg => msg.EbmsRefToMessageId == messageId)
                .Select(msg => msg.MessageBody)
                .FirstOrDefaultAsync();
        }
    }
}