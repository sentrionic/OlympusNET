using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;

namespace Domain
{
    public class Article
    {
        public int Id { get; set; }

        public string Slug { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Body { get; set; }

        public string Image { get; set; }

        [NotMapped] public int FavoritesCount => ArticleFavorites?.Count ?? 0;

        [NotMapped]
        public List<string> TagList => (ArticleTags?.Select(x => x.TagId) ?? Enumerable.Empty<string>()).ToList();

        [JsonIgnore] public List<ArticleTag> ArticleTags { get; set; }

        [JsonIgnore] public List<ArticleFavorite> ArticleFavorites { get; set; }
        
        [JsonIgnore] public List<ArticleBookmark> ArticleBookmarks { get; set; }

        [JsonIgnore] public User Author { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}