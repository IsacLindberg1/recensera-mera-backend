namespace RecenseraMera_API
{
    public class Category
    {
        public int categoryId { get; set; }
        public string category { get; set; } = string.Empty;
        public int reviewId { get; set; }
        public int userId { get; set; }
    }
}
