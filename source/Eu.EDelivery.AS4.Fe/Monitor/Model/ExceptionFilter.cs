using System;
using System.Linq;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Fe.Monitor.Model
{
    /// <summary>
    /// Filter object for exception messages
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Eu.EDelivery.AS4.Fe.Monitor.Model.BaseFilter{Eu.EDelivery.AS4.Entities.ExceptionEntity,
    ///         Eu.EDelivery.AS4.Fe.Monitor.Model.ExceptionMessage}
    ///     </cref>
    /// </seealso>
    public class ExceptionFilter : BaseFilter<ExceptionEntity, ExceptionMessage>
    {
        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        /// <value>
        /// The direction.
        /// </value>
        public Direction[] Direction { get; set; } = { Model.Direction.Inbound, Model.Direction.Outbound };
        /// <summary>
        /// Gets or sets the ebms reference to message identifier.
        /// </summary>
        /// <value>
        /// The ebms reference to message identifier.
        /// </value>
        public string EbmsRefToMessageId { get; set; }       
        /// <summary>
        /// Gets or sets the modification time from.
        /// </summary>
        /// <value>
        /// The modification time from.
        /// </value>
        public DateTime? ModificationTimeFrom { get; set; }
        /// <summary>
        /// Gets or sets the modification time to.
        /// </summary>
        /// <value>
        /// The modification time to.
        /// </value>
        public DateTime? ModificationTimeTo { get; set; }
        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        /// <value>
        /// The operation.
        /// </value>
        public string[] Operation { get; set; }

        /// <summary>
        /// Applies the filter.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public IQueryable<TEntity> ApplyFilter<TEntity>(IQueryable<TEntity> query)
            where TEntity : ExceptionEntity
        {
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
                query = query.Where(qr => qr.ModificationTime >= ModificationTimeFrom && qr.ModificationTime <= ModificationTimeTo);
            }

            switch (InsertionTimeType)
            {
                case DateTimeFilterType.Custom:
                    if (InsertionTimeFrom != null && InsertionTimeTo != null)
                    {
                        query = query.Where(qr => qr.InsertionTime >= InsertionTimeFrom && qr.InsertionTime <= InsertionTimeTo);
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

            if (Operation == null) return query;
            {
                query = query.Where(qr => Operation.Contains(qr.Operation.ToString()));
            }

            return query;
        }
    }
}