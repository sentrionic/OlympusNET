using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Articles
{
    public class Details
    {
        public class Query : IRequest<ArticleResponse>
        {
            public string Slug { get; set; }
        }

        public class Handler : IRequestHandler<Query, ArticleResponse>
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

            public async Task<ArticleResponse> Handle(Query request, CancellationToken cancellationToken)
            {
                var article = await _context.Articles
                    .GetAllData()
                    .Where(x => x.Slug == request.Slug)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken);

                if (article == null)
                    throw new RestException(HttpStatusCode.NotFound, new {Article = "Not found"});

                var currentUser = await _context.Users
                    .SingleOrDefaultAsync(x => x.Username == _userAccessor.GetCurrentUsername());

                return _articleMapper.MapToResponse(article, currentUser);
            }
        }
    }
}