using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using Application.Profile;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Comments
{
    public class Create
    {
        public class Command : IRequest<CommentResponse>
        {
            public string Body { get; set; }
            public string Slug { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Body).NotNull().Length(3, 250);
            }
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
                var article = await _context.Articles
                    .Where(x => x.Slug == request.Slug)
                    .FirstOrDefaultAsync(cancellationToken);

                if (article == null)
                    throw new RestException(HttpStatusCode.NotFound, new {Article = "Not found"});

                var user = await _context.Users
                    .Include(x => x.Followers)
                    .Include(x => x.Followings)
                    .SingleOrDefaultAsync(x =>
                        x.Username == _userAccessor.GetCurrentUsername()
                    );

                var comment = new Comment
                {
                    Author = user,
                    Article = article,
                    Body = request.Body,
                    CreatedAt = DateTime.Now
                };

                await _context.Comments.AddAsync(comment, cancellationToken);

                var success = await _context.SaveChangesAsync(cancellationToken) > 0;

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