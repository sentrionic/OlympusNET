using System.Threading;
using System.Threading.Tasks;
using Application.Articles;
using Application.Bookmarks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ArticlesController : BaseController
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ArticlesEnvelope>> List(
            string tag, string author, 
            string favorited, string cursor,
            string order, string search,
            int? limit, int? p, CancellationToken cancellationToken)
        {
            return await Mediator.Send(
                new List.Query
                (
                    limit, p, tag, author, favorited, search, cursor, order
                ),
                cancellationToken);
        }

        [HttpGet("feed")]
        public async Task<ActionResult<ArticlesEnvelope>> Feed(string cursor, int? limit, int? p,
            CancellationToken cancellationToken)
        {
            return await Mediator.Send(new Feed.Query(limit, p, cursor), cancellationToken);
        }

        [HttpGet("bookmarked")]
        public async Task<ActionResult<ArticlesEnvelope>> Bookmarked(string cursor, int? limit, int? p,
            CancellationToken cancellationToken)
        {
            return await Mediator.Send(new Bookmarked.Query(limit, p, cursor), cancellationToken);
        }

        [HttpGet("{slug}")]
        [AllowAnonymous]
        public async Task<ActionResult<ArticleResponse>> Details(string slug, CancellationToken cancellationToken)
        {
            return await Mediator.Send(new Details.Query {Slug = slug}, cancellationToken);
        }

        [HttpPost]
        public async Task<ActionResult<ArticleResponse>> Create([FromForm] Create.Command command,
            CancellationToken cancellationToken)
        {
            return await Mediator.Send(command, cancellationToken);
        }

        [HttpPut("{slug}")]
        public async Task<ActionResult<ArticleResponse>> Update(string slug, [FromForm] Update.Command command,
            CancellationToken cancellationToken)
        {
            command.Slug = slug;
            return await Mediator.Send(command, cancellationToken);
        }

        [HttpDelete("{slug}")]
        public async Task<ActionResult<ArticleResponse>> Delete(string slug, CancellationToken cancellationToken)
        {
            return await Mediator.Send(new Delete.Command {Slug = slug}, cancellationToken);
        }

        [HttpPost("{slug}/favorite")]
        public async Task<ArticleResponse> FavoriteAdd(string slug, CancellationToken cancellationToken)
        {
            return await Mediator.Send(new Favorite.Command {Slug = slug}, cancellationToken);
        }

        [HttpDelete("{slug}/favorite")]
        public async Task<ArticleResponse> FavoriteDelete(string slug, CancellationToken cancellationToken)
        {
            return await Mediator.Send(new Unfavorite.Command {Slug = slug}, cancellationToken);
        }

        [HttpPost("{slug}/bookmark")]
        public async Task<ArticleResponse> BookmarkAdd(string slug, CancellationToken cancellationToken)
        {
            return await Mediator.Send(new Add.Command {Slug = slug}, cancellationToken);
        }

        [HttpDelete("{slug}/bookmark")]
        public async Task<ArticleResponse> BookmarkDelete(string slug, CancellationToken cancellationToken)
        {
            return await Mediator.Send(new Remove.Command {Slug = slug}, cancellationToken);
        }
    }
}