using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace Domain
{
    public class User
    {
        public int Id { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [JsonIgnore] public List<ArticleFavorite> ArticleFavorites { get; set; }
        
        [JsonIgnore] public List<ArticleBookmark> ArticleBookmarks { get; set; }

        [JsonIgnore] public ICollection<UserFollowing> Followings { get; set; }

        [JsonIgnore] public ICollection<UserFollowing> Followers { get; set; }

        [JsonIgnore] public byte[] Hash { get; set; }

        [JsonIgnore] public byte[] Salt { get; set; }
    }

    public class UserRole : IdentityRole<int>
    {
    }
}