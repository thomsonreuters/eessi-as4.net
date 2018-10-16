using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Fe.Monitor.Model
{
    public class MessageFilter : BaseFilter<MessageEntity, Message>
    {
        public Direction[] Direction { get; set; } = { Model.Direction.Inbound, Model.Direction.Outbound };
        public string EbmsMessageId { get; set; }
        public string EbmsRefToMessageId { get; set; }
        public string[] ContentType { get; set; }
        public string[] Operation { get; set; }
        public DateTime? ModificationTimeFrom { get; set; }
        public DateTime? ModificationTimeTo { get; set; }
        public MessageExchangePattern[] MEP { get; set; }
        public MessageType[] EbmsMessageType { get; set; }
        public string[] Status { get; set; }
        public string FromParty { get; set; }
        public string ToParty { get; set; }
        public bool ShowDuplicates { get; set; }
        public bool ShowTests { get; set; }
        public string ActionName { get; set; }
        public string Service { get; set; }
        public string MPC { get; set; }
        public string[] Pmode { get; set; }

        public IQueryable<TEntity> ApplyFilter<TEntity>(IQueryable<TEntity> query)
            where TEntity : MessageEntity
        {
            if (!String.IsNullOrEmpty(EbmsMessageId))
            {
                string filter = EbmsMessageId.Replace("*", "");
                if (EbmsMessageId.StartsWith("*") && EbmsMessageId.EndsWith("*"))
                {
                    query = query.Where(qr => qr.EbmsMessageId.Contains(filter));
                }
                else if (EbmsMessageId.EndsWith("*"))
                {
                    query = query.Where(qr => qr.EbmsMessageId.StartsWith(filter));
                }
                else if (EbmsMessageId.StartsWith("*"))
                {
                    query = query.Where(qr => qr.EbmsMessageId.EndsWith(filter));
                }
                else
                {
                    query = query.Where(qr => qr.EbmsMessageId == filter);
                }
            }

            if (!String.IsNullOrEmpty(EbmsRefToMessageId))
            {
                string filter = EbmsRefToMessageId.Replace("*", "");
                if (EbmsRefToMessageId.StartsWith("*") && EbmsRefToMessageId.EndsWith("*"))
                {
                    query = query.Where(qr => qr.EbmsRefToMessageId.Contains(filter));
                }
                else if (EbmsRefToMessageId.EndsWith("*"))
                {
                    query = query.Where(qr => qr.EbmsRefToMessageId.StartsWith(filter));
                }
                else if (EbmsRefToMessageId.StartsWith("*"))
                {
                    query = query.Where(qr => qr.EbmsRefToMessageId.EndsWith(filter));
                }
                else
                {
                    query = query.Where(qr => qr.EbmsRefToMessageId == filter);
                }
            }

            if (Operation != null)
            {
                IEnumerable<string> operations = Operation.Select(op => op);
                query = query.Where(qr => operations.Contains(qr.Operation.ToString()));
            }

            switch (InsertionTimeType)
            {
                case DateTimeFilterType.Custom:
                    if (InsertionTimeFrom != null && InsertionTimeTo != null)
                    {
                        query = query.Where(qr => qr.InsertionTime >= InsertionTimeFrom
                                                  && qr.InsertionTime <= InsertionTimeTo);
                    }
                    else if (InsertionTimeFrom == null && InsertionTimeTo != null)
                    {
                        query = query.Where(qr => qr.InsertionTime <= InsertionTimeTo);
                    }
                    else if (InsertionTimeFrom != null && InsertionTimeTo == null)
                    {
                        query = query.Where(qr => qr.InsertionTime >= InsertionTimeFrom);
                    }

                    break;
                case DateTimeFilterType.Last4Hours:
                    DateTime last4Hours = DateTime.UtcNow.AddHours(-4);
                    query = query.Where(x => x.InsertionTime >= last4Hours);
                    break;
                case DateTimeFilterType.LastDay:
                    DateTime lastDay = DateTime.UtcNow.AddDays(-1);
                    query = query.Where(x => x.InsertionTime >= lastDay);
                    break;
                case DateTimeFilterType.LastHour:
                    DateTime lastHour = DateTime.UtcNow.AddHours(-1);
                    query = query.Where(x => x.InsertionTime >= lastHour);
                    break;
                case DateTimeFilterType.LastMonth:
                    DateTime lastMonth = DateTime.UtcNow.AddMonths(-1);
                    query = query.Where(x => x.InsertionTime >= lastMonth);
                    break;
                case DateTimeFilterType.LastWeek:
                    DateTime lastWeek = DateTime.UtcNow.AddDays(-7);
                    query = query.Where(x => x.InsertionTime >= lastWeek);
                    break;
                case DateTimeFilterType.Ignore:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (ModificationTimeFrom != null && ModificationTimeTo == null)
            {
                query = query.Where(qr => qr.ModificationTime >= ModificationTimeFrom);
            }
            else if (ModificationTimeFrom == null && ModificationTimeTo != null)
            {
                query = query.Where(qr => qr.ModificationTime <= ModificationTimeTo);
            }
            else if (ModificationTimeFrom != null && ModificationTimeTo != null)
            {
                query = query.Where(qr => qr.ModificationTime >= ModificationTimeFrom
                                          && qr.ModificationTime <= ModificationTimeTo);
            }

            if (MEP != null)
            {
                query = query.Where(qr => MEP.Contains(qr.MEP));
            }

            if (EbmsMessageType != null)
            {
                query = query.Where(qr => EbmsMessageType.Contains(qr.EbmsMessageType));
            }

            if (ContentType != null)
            {
                List<string> types = ContentType.Select(x => x == "mime" ? "multipart/related" : "application/soap+xml")
                                                .ToList();
                query = query.Where(qr => types.Any(data => qr.ContentType.StartsWith(data)));
            }

            if (Status != null && Status.Any())
            {
                List<string> statusStrings = Status.Select(status => status.ToString()).ToList();
                query = query.Where(qr => statusStrings.Contains(qr.Status));
            }

            if (!String.IsNullOrEmpty(FromParty))
            {
                query = query.Where(x => x.FromParty == FromParty);
            }

            if (!String.IsNullOrEmpty(ToParty))
            {
                query = query.Where(x => x.ToParty == ToParty);
            }

            if (!ShowDuplicates)
            {
                query = query.Where(x => !x.IsDuplicate);
            }

            if (!ShowTests)
            {
                query = query.Where(x => !x.IsTest);
            }

            if (!String.IsNullOrEmpty(ActionName))
            {
                string filter = ActionName.Replace("*", "");
                if (ActionName.StartsWith("*") && ActionName.EndsWith("*"))
                {
                    query = query.Where(qr => qr.Action.Contains(filter));
                }
                else if (ActionName.EndsWith("*"))
                {
                    query = query.Where(qr => qr.Action.StartsWith(filter));
                }
                else if (ActionName.StartsWith("*"))
                {
                    query = query.Where(qr => qr.Action.EndsWith(filter));
                }
                else
                {
                    query = query.Where(qr => qr.Action == filter);
                }
            }

            if (!String.IsNullOrEmpty(Service))
            {
                string filter = Service.Replace("*", "");
                if (Service.StartsWith("*") && Service.EndsWith("*"))
                {
                    query = query.Where(qr => qr.Service.Contains(filter));
                }
                else if (Service.EndsWith("*"))
                {
                    query = query.Where(qr => qr.Service.StartsWith(filter));
                }
                else if (Service.StartsWith("*"))
                {
                    query = query.Where(qr => qr.Service.EndsWith(filter));
                }
                else
                {
                    query = query.Where(qr => qr.Service == filter);
                }
            }

            if (!String.IsNullOrEmpty(MPC))
            {
                string filter = MPC.Replace("*", "");
                if (MPC.StartsWith("*") && MPC.EndsWith("*"))
                {
                    query = query.Where(qr => qr.Mpc.Contains(filter));
                }
                else if (MPC.EndsWith("*"))
                {
                    query = query.Where(qr => qr.Mpc.StartsWith(filter));
                }
                else if (MPC.StartsWith("*"))
                {
                    query = query.Where(qr => qr.Mpc.EndsWith(filter));
                }
                else
                {
                    query = query.Where(qr => qr.Mpc == filter);
                }
            }

            if (Pmode != null && Pmode.Length > 0)
            {
                query = query.Where(qr => Pmode.Contains(qr.PMode));
            }

            return query;
        }

        public IQueryable<Message> ApplyStatusFilter(IQueryable<Message> query)
        {
            if (Status == null || !Status.Any())
            {
                return query;
            }

            List<string> statusFilter = Status.Select(status => status.ToString()).ToList();
            return query.Where(qr => statusFilter.Contains(qr.Status));
        }
    }
}