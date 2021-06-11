using System.Linq;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Application.Articles
{
    public static class ArticleExtensions
    {
        public static IQueryable<Article> GetAllData(this DbSet<Article> articles)
        {
            return articles
                .AsSplitQuery()
                .Include(x => x.ArticleFavorites)
                .Include(x => x.ArticleBookmarks)
                .Include(x => x.ArticleTags)
                .Include(x => x.Author)
                .Include(x => x.Author.Followers)
                .Include(x => x.Author.Followings);
        }
    }
}