namespace RecenseraMera_API
{
    public class User
    {
        public int userId { get; set; }
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public int role { get; set; }
    }
}
