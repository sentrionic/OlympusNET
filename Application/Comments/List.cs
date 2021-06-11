using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Profile;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Comments
{
    public class List
    {
        public class Query : IRequest<List<CommentResponse>>
        {
            public string Slug { get; set; }
        }

        public class Handler : IRequestHandler<Query, List<CommentResponse>>
        {
            private readonly DataContext _context;
            private readonly IProfileReader _profileReader;

            public Handler(DataContext context, IProfileReader profileReader)
            {
                _context = context;
                _profileReader = profileReader;
            }

            public async Task<List<CommentResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var article = await _context.Articles.Where(x => x.Slug == request.Slug)
                    .FirstOrDefaultAsync(cancellationToken);

                if (article == null)
                    throw new RestException(HttpStatusCode.NotFound, new {Article = "Not found"});

                var comments = await _context.Comments
                    .Where(x => x.Article == article)
                    .Include(x => x.Author)
                    .Include(x => x.Author.Followers)
                    .Include(x => x.Author.Followings)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                var list = new List<CommentResponse>();
                comments.ForEach(comment =>
                {
                    list.Add(
                        new CommentResponse
                        {
                            Id = comment.Id,
                            Author = _profileReader.ReadProfile(comment.Author).Result,
                            Body = comment.Body,
                            CreatedAt = comment.CreatedAt
                        }
                    );
                });

                return list;
            }
        }
    }
}