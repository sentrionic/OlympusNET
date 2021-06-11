using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Articles;
using Application.Errors;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Bookmarks
{
    public class Remove
    {
        public class Command : IRequest<ArticleResponse>
        {
            public string Slug { get; set; }
        }

        public class Handler : IRequestHandler<Command, ArticleResponse>
        {
            private readonly IArticleMapper _articleMapper;
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor, IArticleMapper articleMapper)
            {
                _userAccessor = userAccessor;
                _articleMapper = articleMapper;
                _context = context;
            }

            public async Task<ArticleResponse> Handle(Command request, CancellationToken cancellationToken)
            {
                var article = await _context.Articles
                    .GetAllData()
                    .Where(x => x.Slug == request.Slug)
                    .FirstOrDefaultAsync(cancellationToken);

                if (article == null)
                    throw new RestException(HttpStatusCode.NotFound, new {Article = "Could not find article"});

                var user = await _context.Users.SingleOrDefaultAsync(x =>
                    x.Username == _userAccessor.GetCurrentUsername());

                var bookmark = await _context.ArticleBookmarks
                    .SingleOrDefaultAsync(x => x.ArticleId == article.Id &&
                                               x.UserId == user.Id);

                if (bookmark != null)
                {
                    _context.ArticleBookmarks.Remove(bookmark);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                return _articleMapper.MapToResponse(article, user);
            }
        }
    }
}