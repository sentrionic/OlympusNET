using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Articles;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Articles
{
    public class List
    {
        public class Query : IRequest<ArticlesEnvelope>
        {
            public Query(int? limit, int? p, string tag, string author, string favoritedBy, string search, string cursor, string order)
            {
                Limit = limit;
                Page = p;
                Tag = tag;
                Author = author;
                FavoritedBy = favoritedBy;
                Search = search;
                Cursor = cursor;
                Order = order;
            }

            public int? Limit { get; set; }
            public int? Page { get; set; }
            public string Tag { get; set; }
            public string Author { get; set; }
            public string FavoritedBy { get; set; }
            public string Search { get; set; }
            public string Cursor { get; set; }
            public string Order { get; set; }
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
                    .AsQueryable();

                var realLimit = Math.Min(20, request.Limit ?? 20);
                var realLimitPlusOne = realLimit + 1;
                
                if (!string.IsNullOrWhiteSpace(request.Search))
                {
                    var search = request.Search.ToLower();
                    queryable = queryable.Where(x => x.Title.ToLower().Contains(search) || x.Description.ToLower().Contains(search));
                }

                if (!string.IsNullOrWhiteSpace(request.Tag))
                {
                    var tag = await _context.ArticleTags.FirstOrDefaultAsync(x => x.TagId == request.Tag,
                        cancellationToken);
                    if (tag != null)
                        queryable = queryable.Where(x => x.ArticleTags.Select(y => y.TagId).Contains(tag.TagId));
                    else
                        return new ArticlesEnvelope();
                }

                if (!string.IsNullOrWhiteSpace(request.Author))
                {
                    var author =
                        await _context.Users.FirstOrDefaultAsync(x => x.Username == request.Author, cancellationToken);
                    if (author != null)
                        queryable = queryable.Where(x => x.Author == author);
                    else
                        return new ArticlesEnvelope();
                }

                if (!string.IsNullOrWhiteSpace(request.FavoritedBy))
                {
                    var author = await _context.Users.FirstOrDefaultAsync(x => x.Username == request.FavoritedBy,
                        cancellationToken);
                    if (author != null)
                        queryable = queryable.Where(x => x.ArticleFavorites.Any(y => y.UserId == author.Id));
                    else
                        return new ArticlesEnvelope();
                }
                
                if (!string.IsNullOrWhiteSpace(request.Cursor))
                {
                    queryable = queryable.Where(x => x.CreatedAt < DateTime.Parse(request.Cursor));
                }

                var order = request.Order ?? "DESC";
                queryable = order switch
                {
                    "ASC" => queryable.OrderBy(x => x.CreatedAt),
                    "TOP" => queryable.OrderBy(x => x.ArticleFavorites.Count),
                    _ => queryable.OrderByDescending(x => x.CreatedAt)
                };

                var skip = request.Page - 1 ?? 0;

                var articles = await queryable
                    .Skip(Math.Max(skip, 0) * realLimit)
                    .Take(realLimitPlusOne)
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