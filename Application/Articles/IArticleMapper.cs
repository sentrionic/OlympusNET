using Domain;

namespace Application.Articles
{
    public interface IArticleMapper
    {
        public ArticleResponse MapToResponse(Article article, Domain.User user);
    }
}