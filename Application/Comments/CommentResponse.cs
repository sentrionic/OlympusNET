using System;

namespace Application.Comments
{
    public class CommentResponse
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public Profile.Profile Author { get; set; }
    }
}