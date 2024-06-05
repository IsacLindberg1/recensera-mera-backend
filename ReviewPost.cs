namespace RecenseraMera_API
{
    public class ReviewPost
    {
        public string title { get; set; } = string.Empty;
        public string content { get; set; } = string.Empty;
        //public List<Category> categories { get; set; } = new List<Category> { new Category() };
        public string[] categories { get; set; } = new string[0];
        public string imgUrl { get; set; } = string.Empty;
    }
}
