using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Articles
{
    public class Bookmarked
    {
        public class Query : IRequest<ArticlesEnvelope>
        {
            public Query(int? limit, int? p, string cursor)
            {
                Limit = limit;
                Page = p;
                Cursor = cursor;
            }

            public int? Limit { get; set; }
            public int? Page { get; set; }
            public string Cursor { get; set; }
        }

        public class Handler : IRequestHandler<Query, ArticlesEnvelope>
        {
            private readonly IArticleMapper _articleMapper;
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor, IArticleMapper articleMapper)
            {
                _context = context;
                _userAccessor = userAccessor;
                _articleMapper = articleMapper;
            }

            public async Task<ArticlesEnvelope> Handle(Query request, CancellationToken cancellationToken)
            {
                var user = await _context.Users
                    .SingleOrDefaultAsync(x => x.Username == _userAccessor.GetCurrentUsername());

                var queryable = _context.Articles
                    .GetAllData()
                    .Where(x => x.ArticleBookmarks.Any(y => y.UserId == user.Id))
                    .AsQueryable();

                if (request.Cursor != null)
                {
                    queryable = queryable.Where(x => x.CreatedAt < DateTime.Parse(request.Cursor));
                }

                var realLimit = Math.Min(20, request.Limit ?? 20);
                var realLimitPlusOne = realLimit + 1;

                var skip = request.Page ?? 0;

                var articles = await queryable
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip(Math.Max(skip - 1, 0) * realLimit)
                    .Take(request.Limit ?? 20)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                var list = new List<ArticleResponse>();
                articles.ForEach(article => { list.Add(_articleMapper.MapToResponse(article, user)); });

                return new ArticlesEnvelope
                {
                    Articles = list.Take(realLimit).ToList(),
                    HasMore = list.Count == realLimitPlusOne
                };
            }
        }
    }
}