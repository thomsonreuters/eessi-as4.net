using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public abstract class BaseFilter<TInput, TOutput>
    {
        public int Page { get; set; } = 1;
        public int ResultsPerPage { get; } = 2;
        public Dictionary<string, SortOrder> Sort { get; } = new Dictionary<string, SortOrder>();
        public IQueryable<TInput> ApplyPaging(IQueryable<TInput> query)
        {
            return query.Skip(ResultsPerPage * (Page - 1)).Take(ResultsPerPage);
        }

        public abstract IQueryable<TInput> ApplyFilter(IQueryable<TInput> query);

        public async Task<MessageResult<TOutput>> ToResult(IQueryable<TInput> query)
        {
            query = ApplyFilter(query);
            var count = await query.CountAsync();
            return new MessageResult<TOutput>
            {
                Messages = await ApplyPaging(query).ProjectToListAsync<TOutput>(),
                Total = count,
                Pages = (int)Math.Ceiling((decimal)(count / ResultsPerPage)),
                Page = Page == 0 ? 1 : Page
            };
        }
    }
}