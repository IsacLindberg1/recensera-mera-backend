namespace RecenseraMera_API
{
    public class Comment
    {
        public int userId { get; set; }
        public int reviewId { get; set; }
        public string comment { get; set; } = string.Empty;
        public int commentId { get; set; }
        public string username { get; set; } = string.Empty;
    }
}
