using AutoMapper.QueryableExtensions;
using EnsureThat;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Fe.Pmodes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Eu.EDelivery.AS4.Fe.Monitor.Model;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    /// <summary>
    /// Service to view messages
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Monitor.IMonitorService" />
    public class MonitorService : IMonitorService
    {
        private readonly DatastoreContext context;
        private readonly IAs4PmodeSource pmodeSource;
        private readonly IDatastoreRepository datastoreRepository;
        private readonly MapperConfiguration mapperConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="MonitorService" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="pmodeSource">The pmode source.</param>
        /// <param name="datastoreRepository">The datastore repository.</param>
        /// <param name="mapperConfig">The mapper configuration.</param>
        public MonitorService(DatastoreContext context, IAs4PmodeSource pmodeSource, IDatastoreRepository datastoreRepository, MapperConfiguration mapperConfig)
        {
            this.context = context;
            this.pmodeSource = pmodeSource;
            this.datastoreRepository = datastoreRepository;
            this.mapperConfig = mapperConfig;
        }

        /// <summary>
        /// Gets the exceptions.
        /// </summary>
        /// <param name="filter">Exception filter object</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">filter - Filter must be supplied
        /// or
        /// Direction - Direction cannot be null</exception>
        /// <exception cref="Eu.EDelivery.AS4.Fe.BusinessException">Could not get any exceptions, something went wrong.</exception>
        public async Task<MessageResult<ExceptionMessage>> GetExceptions(ExceptionFilter filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter), @"Filter must be supplied");
            if (filter.Direction == null) throw new ArgumentNullException(nameof(filter.Direction), @"Direction cannot be null");
            var inExceptions = filter.Direction.Contains(Direction.Inbound) ? filter.ApplyFilter(context.InExceptions).ProjectTo<ExceptionMessage>(mapperConfig) : null;
            var outExceptions = filter.Direction.Contains(Direction.Outbound) ? filter.ApplyFilter(context.OutExceptions).ProjectTo<ExceptionMessage>(mapperConfig) : null;

            IQueryable<ExceptionMessage> result = null;
            if (inExceptions != null && outExceptions != null) result = inExceptions.Concat(outExceptions);
            else if (inExceptions != null) result = inExceptions;
            else if (outExceptions != null) result = outExceptions;

            if (result == null) throw new BusinessException("Could not get any exceptions, something went wrong.");

            return ConvertPmodeXmlToNumbers(await filter.ToResult(result.OrderByDescending(msg => msg.InsertionTime)));
        }

        /// <summary>
        /// Gets the messages.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// filter - Filter cannot be null
        /// or
        /// Direction - Direction filter cannot be empty
        /// </exception>
        /// <exception cref="Eu.EDelivery.AS4.Fe.BusinessException">No messages found</exception>
        public async Task<MessageResult<Message>> GetMessages(MessageFilter filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter), @"Filter cannot be null");
            if (filter.Direction == null) throw new ArgumentNullException(nameof(filter.Direction), @"Direction filter cannot be empty");

            IQueryable<InMessage> inMessageQuery = context.InMessages;
            IQueryable<OutMessage> outMessageQuery = context.OutMessages;

            var inMessages = filter.Direction.Contains(Direction.Inbound) ? filter.ApplyFilter(inMessageQuery).ProjectTo<Message>(mapperConfig) : null;
            var outMessages = filter.Direction.Contains(Direction.Outbound) ? filter.ApplyFilter(outMessageQuery).ProjectTo<Message>(mapperConfig) : null;

            IQueryable<Message> result = null;

            if (inMessages != null && outMessages != null) result = inMessages.Concat(outMessages);
            else if (inMessages != null) result = inMessages;
            else if (outMessages != null) result = outMessages;
            if (result == null) throw new BusinessException("No messages found");

            var returnValue = ConvertPmodeXmlToNumbers(await filter.ToResult(filter.ApplyStatusFilter(result).OrderByDescending(msg => msg.InsertionTime)));
            UpdateHasExceptions(returnValue, await GetExceptionIds(returnValue));

            return returnValue;
        }

        /// <summary>
        /// Gets the related messages.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <returns></returns>
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
                    .ProjectTo<Message>(mapperConfig));

                resultTest.Add(context
                    .OutMessages
                    .Where(message => message.EbmsMessageId == refToMessageId)
                    .ProjectTo<Message>(mapperConfig));
            }

            if (!string.IsNullOrEmpty(messageId))
            {
                resultTest.Add(context
                    .InMessages
                    .Where(message => message.EbmsRefToMessageId == messageId)
                    .ProjectTo<Message>(mapperConfig));

                resultTest.Add(context
                    .OutMessages
                    .Where(message => message.EbmsRefToMessageId == messageId)
                    .ProjectTo<Message>(mapperConfig));
            }

            var result = resultTest.First();
            result = resultTest.Skip(1).Aggregate(result, (current, query) => current.Union(query));

            return ConvertPmodeXmlToNumbers(new MessageResult<Message>
            {
                Messages = await result.ToListAsync(),
                Total = await result.CountAsync(),
                Page = 0,
                Pages = 0,
                CurrentPage = 0
            });
        }


        /// <summary>
        /// Gets the pmode number.
        /// </summary>
        /// <param name="pmode">The pmode.</param>
        /// <returns></returns>
        public string GetPmodeNumber(string pmode)
        {
            return string.IsNullOrEmpty(pmode) ? string.Empty : pmodeSource.GetPmodeNumber(pmode);
        }

        /// <summary>
        /// Downloads the message body.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">messageId - messageId parameter cannot be null</exception>
        /// <exception cref="InvalidEnumArgumentException">direction</exception>
        public async Task<Stream> DownloadMessageBody(Direction direction, string messageId)
        {
            if (string.IsNullOrEmpty(messageId)) throw new ArgumentNullException(nameof(messageId), @"messageId parameter cannot be null");
            if (!Enum.IsDefined(typeof(Direction), direction)) throw new InvalidEnumArgumentException(nameof(direction), (int)direction, typeof(Direction));

            if (direction == Direction.Inbound)
            {
                return await datastoreRepository.GetInMessageData(messageId, x => x.RetrieveMessageBody(Registry.Instance.MessageBodyStore));
            }

            return await datastoreRepository.GetOutMessageData(messageId, x => x.RetrieveMessageBody(Registry.Instance.MessageBodyStore));
        }

        /// <summary>
        /// Downloads the exception body.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="id"></param>
        /// <returns>The exception</returns>
        /// <exception cref="ArgumentNullException">messageId - messageId parameter cannot be null</exception>
        public async Task<string> DownloadExceptionMessageBody(Direction direction, long id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), @"Invalid value for id");
            if (!Enum.IsDefined(typeof(Direction), direction)) throw new InvalidEnumArgumentException(nameof(direction), (int)direction, typeof(Direction));
            byte[] body;
            if (direction == Direction.Inbound)
            {
                body = await context
                    .InExceptions
                    .Where(msg => msg.Id == id)
                    .Select(msg => msg.MessageBody)
                    .FirstOrDefaultAsync();
            }
            else
            {
                body = await context
                    .OutExceptions
                    .Where(msg => msg.Id == id)
                    .Select(msg => msg.MessageBody)
                    .FirstOrDefaultAsync();
            }

            return body != null ? Encoding.UTF8.GetString(body) : string.Empty;
        }

        /// <summary>
        /// Gets the exception detail.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <returns></returns>
        public async Task<string> GetExceptionDetail(Direction direction, long messageId)
        {
            if (direction == Direction.Inbound)
            {
                return await context.InExceptions.Where(x => x.Id == messageId).Select(x => x.Exception).FirstOrDefaultAsync();
            }

            return await context.OutExceptions.Where(x => x.Id == messageId).Select(x => x.Exception).FirstOrDefaultAsync();
        }

        private static void UpdateHasExceptions(MessageResult<Message> returnValue, List<string> exceptionIds)
        {
            returnValue.Messages = returnValue.Messages.Select(x =>
            {
                x.HasExceptions = exceptionIds.Any(ex => ex == x.EbmsMessageId);
                return x;
            });
        }

        private async Task<List<string>> GetExceptionIds(MessageResult<Message> returnValue)
        {
            var ids = returnValue.Messages.Select(msg => msg.EbmsMessageId).ToList();

            var inExceptions = context.InExceptions.Where(ex => ids.Contains(ex.EbmsRefToMessageId)).Select(ex => ex.EbmsRefToMessageId);
            var outExceptions = context.OutExceptions.Where(ex => ids.Contains(ex.EbmsRefToMessageId)).Select(ex => ex.EbmsRefToMessageId);

            return await inExceptions.Union(outExceptions).ToListAsync();
        }

        private MessageResult<Message> ConvertPmodeXmlToNumbers(MessageResult<Message> result)
        {
            foreach (var message in result.Messages) message.PMode = GetPmodeNumber(message.PMode);
            return result;
        }

        private MessageResult<ExceptionMessage> ConvertPmodeXmlToNumbers(MessageResult<ExceptionMessage> result)
        {
            foreach (var message in result.Messages) message.PMode = GetPmodeNumber(message.PMode);
            return result;
        }
    }
}