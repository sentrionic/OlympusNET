using Domain;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ArticleTag> ArticleTags { get; set; }
        public DbSet<ArticleFavorite> ArticleFavorites { get; set; }
        public DbSet<ArticleBookmark> ArticleBookmarks { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<UserFollowing> Followings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ArticleTag>(b =>
            {
                b.HasKey(t => new {t.ArticleId, t.TagId});

                b.HasOne(pt => pt.Article)
                    .WithMany(p => p.ArticleTags)
                    .HasForeignKey(pt => pt.ArticleId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(pt => pt.Tag)
                    .WithMany(t => t.ArticleTags)
                    .HasForeignKey(pt => pt.TagId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ArticleFavorite>(b =>
            {
                b.HasKey(t => new {t.ArticleId, t.UserId});

                b.HasOne(pt => pt.Article)
                    .WithMany(p => p.ArticleFavorites)
                    .HasForeignKey(pt => pt.ArticleId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(pt => pt.User)
                    .WithMany(t => t.ArticleFavorites)
                    .HasForeignKey(pt => pt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ArticleBookmark>(b =>
            {
                b.HasKey(t => new {t.ArticleId, t.UserId});

                b.HasOne(pt => pt.Article)
                    .WithMany(p => p.ArticleBookmarks)
                    .HasForeignKey(pt => pt.ArticleId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(pt => pt.User)
                    .WithMany(t => t.ArticleBookmarks)
                    .HasForeignKey(pt => pt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<UserFollowing>(b =>
            {
                b.HasKey(k => new {k.ObserverId, k.TargetId});

                b.HasOne(o => o.Observer)
                    .WithMany(f => f.Followings)
                    .HasForeignKey(o => o.ObserverId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(o => o.Target)
                    .WithMany(f => f.Followers)
                    .HasForeignKey(o => o.TargetId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}