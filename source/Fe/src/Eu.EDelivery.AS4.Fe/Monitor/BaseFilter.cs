using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public abstract class BaseFilter<TInput, TOutput>
    {
        public int Page { get; set; } = 1;
        public int ResultsPerPage { get; } = 50;
        public Dictionary<string, SortOrder> Sort { get; } = new Dictionary<string, SortOrder>();
        public IQueryable<TOutput> ApplyPaging(IQueryable<TOutput> query)
        {
            return query.Skip(ResultsPerPage * (Page - 1)).Take(ResultsPerPage);
        }

        public abstract IQueryable<TInput> ApplyFilter(IQueryable<TInput> query);

        public async Task<MessageResult<TOutput>> ToResult(IQueryable<TInput> query)
        {
            query = ApplyFilter(query);
            var count = await query.CountAsync();
            var result = await ApplyPaging(query.ProjectTo<TOutput>()).ToListAsync();
            return new MessageResult<TOutput>
            {
                Messages = result, // Why not use ProjectToAsync here ? Well because it throws an exception telling that the IQueryable doesn't implement IDbAsync...
                Total = count,
                Pages = (int)Math.Ceiling((decimal)count / (decimal)ResultsPerPage),
                Page = Page == 0 ? 1 : Page
            };
        }
    }
}