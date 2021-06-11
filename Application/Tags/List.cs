using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Tags
{
    public class List
    {
        public class Query : IRequest<List<string>>
        {
        }

        public class QueryHandler : IRequestHandler<Query, List<string>>
        {
            private readonly DataContext _context;

            public QueryHandler(DataContext context)
            {
                _context = context;
            }

            public async Task<List<string>> Handle(Query message, CancellationToken cancellationToken)
            {
                var tags = await _context.Tags
                    .OrderBy(x => x.TagId)
                    .Take(10)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
                return tags.Select(x => x.TagId).ToList();
            }
        }
    }
}