using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Articles
{
    public class Create
    {
        public class Command : IRequest<ArticleResponse>
        {
            public string Title { get; set; }

            public string Description { get; set; }

            public string Body { get; set; }

            public IFormFile Image { get; set; }

            public string[] TagList { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Title).NotNull().Length(10, 100);
                RuleFor(x => x.Description).NotNull().Length(10, 150);
                RuleFor(x => x.Body).NotNull().NotEmpty();
                RuleFor(x => x.TagList).NotNull();
                RuleFor(x => x.TagList.Length).LessThanOrEqualTo(5).When(x => x.TagList != null);
                RuleForEach(x => x.TagList).NotNull().Length(3, 15);
            }
        }

        public class Handler : IRequestHandler<Command, ArticleResponse>
        {
            private readonly DataContext _context;
            private readonly IKeyProvider _keyProvider;
            private readonly IPhotoAccessor _photoAccessor;
            private readonly IArticleMapper _mapper;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IKeyProvider keyProvider, IUserAccessor userAccessor,
                 IPhotoAccessor photoAccessor, IArticleMapper mapper)
            {
                _context = context;
                _keyProvider = keyProvider;
                _userAccessor = userAccessor;
                _photoAccessor = photoAccessor;
                _mapper = mapper;
            }

            public async Task<ArticleResponse> Handle(Command request, CancellationToken cancellationToken)
            {
                var tags = new List<Tag>();
                foreach (var tag in request.TagList ?? Enumerable.Empty<string>())
                {
                    var t = await _context.Tags.FindAsync(tag);
                    if (t == null)
                    {
                        t = new Tag {TagId = tag};
                        await _context.Tags.AddAsync(t, cancellationToken);
                        //save immediately for reuse
                        await _context.SaveChangesAsync(cancellationToken);
                    }

                    tags.Add(t);
                }

                var user = await _context.Users
                    .Include(x => x.Followers)
                    .Include(x => x.Followings)
                    .SingleOrDefaultAsync(x =>
                        x.Username == _userAccessor.GetCurrentUsername());

                var url = $"https://picsum.photos/seed/{_keyProvider.GetUniqueKey(6)}/1080";

                if (request.Image != null)
                {
                    var directory = $"dotnet/{user.Id}/{_keyProvider.GetUniqueKey(18)}";
                    url = await _photoAccessor.AddArticleImage(request.Image, directory);
                }

                var article = new Article
                {
                    Title = request.Title,
                    Description = request.Description,
                    Body = request.Body,
                    Slug = _keyProvider.GenerateSlug(request.Title),
                    Image = url,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Author = user
                };

                await _context.Articles.AddAsync(article, cancellationToken);

                await _context.ArticleTags.AddRangeAsync(tags.Select(x => new ArticleTag
                {
                    Article = article,
                    Tag = x
                }), cancellationToken);

                var success = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!success) throw new Exception("Problem saving changes");
                {
                    var created = await _context.Articles.GetAllData().Where(x => x.Slug == article.Slug)
                        .FirstOrDefaultAsync(cancellationToken);

                    return _mapper.MapToResponse(created, created.Author);
                }

            }
        }
    }
}