using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public abstract class BaseFilter<TInput, TOutput>
    {
        public int Page { get; set; } = 1;
        public int ResultsPerPage { get; } = 50;
        public IQueryable<TOutput> ApplyPaging(IQueryable<TOutput> query)
        {
            return query.Skip(ResultsPerPage * (Page - 1)).Take(ResultsPerPage);
        }

        public abstract IQueryable<TInput> ApplyFilter(IQueryable<TInput> query);

        public async Task<MessageResult<TOutput>> ToResult(IQueryable<TOutput> query)
        {
            var count = await query.CountAsync();
            var result = await ApplyPaging(query).ToListAsync();
            return new MessageResult<TOutput>
            {
                Messages = result,
                Total = count,
                Pages = (int)Math.Ceiling((decimal)count / (decimal)ResultsPerPage),
                Page = Page == 0 ? 1 : Page
            };
        }
    }
}