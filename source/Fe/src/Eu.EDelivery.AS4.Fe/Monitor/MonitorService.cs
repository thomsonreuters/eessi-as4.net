using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Fe.Pmodes;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using EnsureThat;

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

    public async Task<MessageResult<ExceptionMessage>> GetOutExceptions(OutExceptionFilter filter)
    {
      return ConvertPmodeXmlToNumbers(await filter.ToResult(context.OutExceptions));
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

    public async Task<MessageResult<ExceptionMessage>> GetInExceptions(InExceptionFilter filter)
    {
      return ConvertPmodeXmlToNumbers(await filter.ToResult(context.InExceptions));
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

    public string GetPmodeNumber(string pmode)
    {
      return string.IsNullOrEmpty(pmode) ? string.Empty : pmodeSource.GetPmodeNumber(pmode);
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