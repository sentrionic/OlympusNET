using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using Application.Profile;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Comments
{
    public class Delete
    {
        public class Command : IRequest<CommentResponse>
        {
            public int CommentId { get; set; }
        }

        public class Handler : IRequestHandler<Command, CommentResponse>
        {
            private readonly DataContext _context;
            private readonly IProfileReader _profileReader;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor, IProfileReader profileReader)
            {
                _context = context;
                _userAccessor = userAccessor;
                _profileReader = profileReader;
            }

            public async Task<CommentResponse> Handle(Command request, CancellationToken cancellationToken)
            {
                var comment = await _context.Comments.FindAsync(request.CommentId);

                if (comment == null)
                    throw new RestException(HttpStatusCode.NotFound, new {Comment = "Not found"});

                var user = await _context.Users
                    .Include(x => x.Followers)
                    .Include(x => x.Followings)
                    .SingleOrDefaultAsync(x =>
                        x.Username == _userAccessor.GetCurrentUsername()
                    );

                if (user.Id != comment.Author.Id)
                    throw new RestException(HttpStatusCode.Unauthorized);

                _context.Remove(comment);

                var success = await _context.SaveChangesAsync() > 0;

                if (success)
                    return new CommentResponse
                    {
                        Id = comment.Id,
                        Author = _profileReader.ReadProfile(comment.Author).Result,
                        Body = comment.Body,
                        CreatedAt = comment.CreatedAt
                    };

                throw new Exception("Problem saving changes");
            }
        }
    }
}