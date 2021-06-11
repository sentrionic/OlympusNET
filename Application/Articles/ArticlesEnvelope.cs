using System.Collections.Generic;

namespace Application.Articles
{
    public class ArticlesEnvelope
    {
        public ArticlesEnvelope()
        {
            Articles = new List<ArticleResponse>();
            HasMore = false;
        }

        public List<ArticleResponse> Articles { get; set; }
        public bool HasMore { get; set; }
    }
}