using System;
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
    public class Delete
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

            public Handler(DataContext context, IArticleMapper articleMapper, IUserAccessor userAccessor)
            {
                _context = context;
                _articleMapper = articleMapper;
                _userAccessor = userAccessor;
            }

            public async Task<ArticleResponse> Handle(Command request, CancellationToken cancellationToken)
            {
                var article = await _context.Articles
                    .GetAllData()
                    .Where(x => x.Slug == request.Slug)
                    .FirstOrDefaultAsync(cancellationToken);

                if (article == null)
                    throw new RestException(HttpStatusCode.NotFound, new {Article = "Not found"});

                var user = await _context.Users
                    .AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Username == _userAccessor.GetCurrentUsername());

                if (user.Id != article.Author.Id)
                    throw new RestException(HttpStatusCode.Unauthorized);

                var comments = await _context.Comments.Where(x => x.Article == article).ToListAsync(cancellationToken);

                _context.RemoveRange(comments);

                _context.Remove(article);

                var success = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (success) return _articleMapper.MapToResponse(article, user);

                throw new Exception("Problem saving changes");
            }
        }
    }
}