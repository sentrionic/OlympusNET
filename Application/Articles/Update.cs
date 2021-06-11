using System;
using System.Collections.Generic;
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
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Articles
{
    public class Update
    {
        public class Command : IRequest<ArticleResponse>
        {
            public string Slug { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Body { get; set; }
            public string[] TagList { get; set; }
            public IFormFile Image { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Title).Length(10, 100);
                RuleFor(x => x.Description).Length(10, 150);
                RuleFor(x => x.Body);
                RuleFor(x => x.TagList);
                RuleFor(x => x.TagList.Length).LessThanOrEqualTo(5).When(x => x.TagList != null);
                RuleForEach(x => x.TagList).NotNull().Length(3, 15);
            }
        }

        public class Handler : IRequestHandler<Command, ArticleResponse>
        {
            private readonly IArticleMapper _articleMapper;
            private readonly DataContext _context;
            private readonly IKeyProvider _keyProvider;
            private readonly IPhotoAccessor _photoAccessor;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor, IProfileReader profileReader,
                IPhotoAccessor photoAccessor, IKeyProvider keyProvider, IArticleMapper articleMapper)
            {
                _context = context;
                _userAccessor = userAccessor;
                _photoAccessor = photoAccessor;
                _keyProvider = keyProvider;
                _articleMapper = articleMapper;
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
                    .SingleOrDefaultAsync(x => x.Username == _userAccessor.GetCurrentUsername());

                if (article.Author.Id != user.Id)
                    throw new RestException(HttpStatusCode.Unauthorized);

                article.Title = request.Title ?? article.Title;
                article.Description = request.Description ?? article.Description;
                article.Body = request.Body ?? article.Body;
                article.UpdatedAt = DateTime.UtcNow;

                if (request.Image != null)
                {
                    _photoAccessor.DeletePhoto(article.Image);
                    var directory = $"dotnet/{user.Id}/{_keyProvider.GetUniqueKey(18)}";
                    article.Image = await _photoAccessor.AddArticleImage(request.Image, directory);
                }

                if (request.TagList != null)
                {
                    var articleTagsToCreate = GetArticleTagsToCreate(article, request.TagList);
                    var articleTagsToDelete = GetArticleTagsToDelete(article, request.TagList);

                    // add the new article tags
                    await _context.ArticleTags.AddRangeAsync(articleTagsToCreate, cancellationToken);
                    // delete the tags that do not exist anymore
                    _context.ArticleTags.RemoveRange(articleTagsToDelete);
                }

                var success = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (success) return _articleMapper.MapToResponse(article, user);

                throw new Exception("Problem saving changes");
            }

            private static IEnumerable<ArticleTag> GetArticleTagsToCreate(Article article,
                IEnumerable<string> articleTagList)
            {
                var articleTagsToCreate = new List<ArticleTag>();
                foreach (var tag in articleTagList)
                {
                    var at = article.ArticleTags.FirstOrDefault(t => t.TagId == tag);
                    if (at != null) continue;
                    at = new ArticleTag
                    {
                        Article = article,
                        ArticleId = article.Id,
                        Tag = new Tag {TagId = tag},
                        TagId = tag
                    };
                    articleTagsToCreate.Add(at);
                }

                return articleTagsToCreate;
            }

            private static IEnumerable<ArticleTag> GetArticleTagsToDelete(Article article,
                IEnumerable<string> articleTagList)
            {
                return (from tag in article.ArticleTags
                    let at = articleTagList.FirstOrDefault(t => t == tag.TagId)
                    where at == null
                    select tag).ToList();
            }
        }
    }
}