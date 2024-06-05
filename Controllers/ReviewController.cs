using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace RecenseraMera_API.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class ReviewController : Controller
    {
        MySqlConnection connection;

        public ReviewController(IConfiguration config)
        {
            string ip = config["ip"];
            connection = new MySqlConnection(ip);
        }

        [HttpPost("CreateReview")]
        public ActionResult CreateReview(ReviewPost review)
        {           
            //GÖR TILL FUNKTION
            string authorization = Request.Headers["Authorization"];
            User user = (User)UserController.sessionId[authorization];

            const string DIRECTORY = "C:\\Users\\Elev\\Documents\\GitHub\\recensera-mera-frontend-ny\\recensera-mera\\public\\pictures\\";
            review.imgUrl = review.imgUrl.Split(',')[1];
            byte[] data = Convert.FromBase64String(review.imgUrl);
            string randomBase64 = GenerateRandomBase64String();
            string path = DIRECTORY + randomBase64 + ".png";          
            
            try
            {
                System.IO.File.WriteAllBytes(path, data);
            }
            catch (Exception exception)
            {
                return StatusCode(500, exception.Message);
            }
            
            try
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.Prepare();
                command.CommandText = "INSERT INTO `review` (`content`, `title`, `imgUrl`) VALUES (@content, @title, @imgUrl)";
                command.Parameters.AddWithValue("@content", review.content);
                command.Parameters.AddWithValue("@title", review.title);
                command.Parameters.AddWithValue("@imgUrl", randomBase64);
                int rows = command.ExecuteNonQuery();

                CreateCategories(review);

                if (rows == 0)
                {
                    System.IO.File.Delete(path);
                    connection.Close();
                    return StatusCode(500, "Image rows was zero");
                }
            }

            catch(Exception exception)
            {
                System.IO.File.Delete(path);
                connection.Close();
                return StatusCode(500, exception.Message);
            }
            connection.Close();
            return StatusCode(201, "Lyckades skapa Recension");
        }

        [HttpGet("ViewAllReviews")]
        public ActionResult<List<Review>> ViewAllReviews()
        {

            List<Review> reviews = new List<Review>();
            
            try
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.Prepare();
                command.CommandText = "SELECT `reviewId`, t1.`userId`, `content`, `title`, `imgUrl` FROM `review` t1 LEFT JOIN `user` t2 ON t1.userId = t2.userId;";
                MySqlDataReader data = command.ExecuteReader();

                while (data.Read())
                {
                    Review review = new Review();
                    review.reviewId = data.GetInt32("reviewId");
                    review.userId = data.GetInt32("userId");              
                    review.content = data.GetString("content");
                    review.title = data.GetString("title");
                    review.imgUrl = data.GetString("imgUrl");
                    reviews.Add(review);
                }
                data.Close();

                foreach (Review review in reviews)
                {
                    review.username = GetUsername(review.userId); 
                    review.comments = GetComments(review.reviewId);
                    review.categories = GetCategories(review.reviewId);
                    review.likeCount = GetLikes(review);
                }
            }
            catch (Exception exception)
            {
                connection.Close();
                return StatusCode(500, exception.Message);
            }
            connection.Close();
            return StatusCode(200, reviews);
        }

        private List<Comment> GetComments(int reviewId)
        {
            List<Comment> comments = new List<Comment>();
            try
            {
                MySqlCommand command = connection.CreateCommand();
                command.Prepare();
                command.CommandText = "SELECT * FROM comment WHERE reviewId = @reviewId";
                command.Parameters.AddWithValue("@reviewId", reviewId);
                MySqlDataReader reader = command.ExecuteReader();
                                
                while (reader.Read())
                {
                    Comment comment = new Comment();                 
                    comment.userId = reader.GetUInt16("userId");
                    comment.reviewId = reader.GetUInt16("reviewId");
                    comment.comment = reader.GetString("comment");
                    comment.commentId = reader.GetUInt16("commentId");
                    comments.Add(comment);
                }
                reader.Close();

                foreach(Comment comment in comments)
                {
                    comment.username = GetUsername(comment.userId);
                }

            }
            catch(Exception exception)
            {
                Console.WriteLine($"Gick inte att hämta kommentar! fel:{exception.Message}");               
            }

            return comments;
        }

        private List<Category> GetCategories(int reviewId)
        {
            List<Category> categories = new List<Category>();
            try
            {
                MySqlCommand command = connection.CreateCommand();
                command.Prepare();
                command.CommandText = "SELECT * FROM category WHERE reviewId = @reviewId";
                command.Parameters.AddWithValue("@reviewId", reviewId);
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Category category = new Category();
                    category.categoryId = reader.GetInt32("categoryId");
                    category.category = reader.GetString("category");
                    category.reviewId = reader.GetUInt16("reviewId");

                    categories.Add(category);
                }
                reader.Close();

            }
            catch (Exception exception)
            {
                Console.WriteLine($"Gick inte att hämta kategori! fel:{exception.Message}");               
            }
            
            return categories;
        }


        private List<Category> CreateCategories(ReviewPost review)
        {
            List<Category> categories = new List<Category>();

            foreach(string category in review.categories)
            {
                categories.Add(new Category { category = category});

                try
                {
                    MySqlCommand command = connection.CreateCommand();
                    command.Prepare();
                    command.CommandText = "INSERT INTO category (reviewId, category) SELECT MAX(reviewId), @category FROM review";
                    command.Parameters.AddWithValue("@category", category);
                    command.ExecuteNonQuery();
                    Console.WriteLine("Lyckades skapa kategori");
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Kunde inte skapa en kategori! {exception.Message}");
                }
            }

            if(categories.Count < 1)
            {
                Console.WriteLine("Inga categorier ):");
            }

            return categories;
        }

        private int GetLikes(Review review)
        {
            int likeCount = 0;
            try
            {
                MySqlCommand command = connection.CreateCommand();
                command.Prepare(); //'like WHERE reviewId = 1' 
                command.CommandText = "SELECT COUNT(userId) AS likeCount FROM likes WHERE reviewId = @reviewId";
                command.Parameters.AddWithValue("@reviewId", review.reviewId);
                MySqlDataReader data = command.ExecuteReader();
                data.Read();
                likeCount = data.GetInt32("likeCount");
                data.Close();
                Console.WriteLine("Lyckades hämta likes");
            }
            catch(Exception exception)
            {
                Console.WriteLine("Gick ej att hämta likes" + exception.Message);
            }

            return likeCount;
        }

        private string GetUsername(int userId)
        {
            string username = "";
            try
            {
                MySqlCommand command = connection.CreateCommand();
                command.Prepare();
                command.CommandText = "SELECT username FROM user WHERE userId = @userId";
                command.Parameters.AddWithValue("@userId", userId);
                MySqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    username = reader.GetString("UserName");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Gick inte att hämta användarnamn!" + exception.Message);
            }

            return username;
        }

        [HttpDelete("RemoveReview")] 
        public ActionResult RemoveReview(Review review)
        {
            string auth = this.HttpContext.Request.Headers["Authorization"];

            if (auth == null || !UserController.sessionId.ContainsKey(auth))
            {
                return StatusCode(403, "0");
            }
            User user = (User)UserController.sessionId[auth];

            if (user.role != 3)
            {
                return StatusCode(403, "Du har inte behörighet!");
            }

            try
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.Prepare();
                command.CommandText = "DELETE FROM `review` WHERE `reviewId` = @id";
                command.Parameters.AddWithValue("@id", review.reviewId);
                command.ExecuteNonQuery();

                connection.Close();
                return StatusCode(200, $"Lyckades ta bort recension med id: {review.reviewId}");
            }
            catch (Exception exception)
            {
                connection.Close();
                return StatusCode(500, exception.Message);
            }
        }

        static string GenerateRandomBase64String()
        {
            byte[] randomBytes = new byte[5];
            new Random().NextBytes(randomBytes);

            string randomBase64 = Convert.ToBase64String(randomBytes);
            return randomBase64.Substring(0, 5);
        }
    }
}
