namespace RecenseraMera_API
{
    public class Review
    {
        public int reviewId { get; set; }
        public int userId { get; set; }
        public string username { get; set; } = string.Empty;
        public string content { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
        public string imgUrl { get; set; } = string.Empty;
        public List<Comment> comments { get; set; } = new List<Comment> { new Comment() };
        public List<Category> categories { get; set; } = new List<Category> { new Category() };
        public int likeCount { get; set; }
    }
}
