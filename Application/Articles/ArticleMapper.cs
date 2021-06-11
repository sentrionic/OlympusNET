using System.Linq;
using Application.Profile;
using Domain;

namespace Application.Articles
{
    public class ArticleMapper : IArticleMapper
    {
        private readonly IProfileReader _profileReader;

        public ArticleMapper(IProfileReader profileReader)
        {
            _profileReader = profileReader;
        }

        public ArticleResponse MapToResponse(Article article, Domain.User user)
        {
            return new ArticleResponse
            {
                Id = article.Id,
                Slug = article.Slug,
                Title = article.Title,
                Description = article.Description,
                Body = article.Body,
                Image = article.Image,
                CreatedAt = article.CreatedAt,
                UpdatedAt = article.UpdatedAt,
                Bookmarked = user != null && article.ArticleBookmarks.Any(x => x.UserId == user.Id),
                Favorited = user != null && article.ArticleFavorites.Any(x => x.UserId == user.Id),
                FavoritesCount = article.FavoritesCount,
                TagList = article.TagList,
                Author = _profileReader.ReadProfile(article.Author).Result,
            };
        }
    }
}