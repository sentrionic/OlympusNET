using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Application.Articles
{
    public class ArticleResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Body { get; set; }
        public string Image { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Slug { get; set; }
        public bool Favorited { get; set; }
        public bool Bookmarked { get; set; }
        public int FavoritesCount { get; set; }

        [JsonPropertyName("tagList")] public ICollection<string> TagList { get; set; }

        [JsonPropertyName("author")] public Profile.Profile Author { get; set; }
    }
}