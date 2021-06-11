using System;

namespace Domain
{
    public class Comment
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public User Author { get; set; }
        public Article Article { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}