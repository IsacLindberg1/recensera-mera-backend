using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace RecenseraMera_API.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class CommentController : Controller
    {
        MySqlConnection connection;

        public CommentController(IConfiguration config)
        {
            string ip = config["ip"];
            connection = new MySqlConnection(ip);
        }

        [HttpPost("CreateComment")] 
        public ActionResult CreateComment(Comment comment)
        {
            string authorization = Request.Headers["Authorization"];
            User user = (User)UserController.sessionId[authorization];

            try
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.Prepare();
                command.CommandText = "INSERT INTO `comment` (`userId`, `reviewId`, `comment`) VALUES(@userId, @reviewId, @comment)";
                command.Parameters.AddWithValue("@userId", comment.userId);
                command.Parameters.AddWithValue("@reviewId", comment.reviewId);
                command.Parameters.AddWithValue("@comment", comment.comment);
                command.ExecuteNonQuery();

                connection.Close();
                return StatusCode(200, $"Lyckades skapa kommentar på recension = {comment.reviewId} Content = {comment.comment}");
            }
            catch (Exception exception)
            {
                connection.Close();
                return StatusCode(500, exception.Message);
            }
        }

        [HttpGet("GetAllComments")]
        public ActionResult<List<Comment>> GetAllComments(Comment comment)
        {
            List<Comment> comments = new List<Comment>();

            try
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.Prepare();
                command.CommandText = "SELECT comment.comment, review.reviewId FROM `comment` JOIN `review` WHERE review.reviewId = comment.reviewId AND review.reviewId = @reviewId";
                command.Parameters.AddWithValue("@reviewId", comment.reviewId);
                MySqlDataReader data = command.ExecuteReader();

                while (data.Read())
                {
                    comment.comment = data.GetString("comment");

                    comments.Add(comment);
                }

                foreach (Comment retrievedComment in comments)
                {
                    retrievedComment.username = GetUsername(comment.userId);
                }

                data.Close();
                return StatusCode(200, comments);
            }
            catch (Exception exception)
            {
                connection.Close();
                return StatusCode(500, exception.Message);
            }
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
                reader.Read();
                username = reader.GetString("username");
                reader.Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine("Gick inte att hämta användarnamn!" + exception.Message);
            }

            return username;
        }

        [HttpDelete("DeleteComment")] 
        public ActionResult DeleteComment(Comment comment)
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
                command.CommandText = "DELETE FROM `comment` WHERE `commentId` = @commentId";
                command.Parameters.AddWithValue("@commentId", comment.commentId);
                command.ExecuteNonQuery();

                connection.Close();
                return StatusCode(200, $"Lyckades ta bort kommentar med id: {comment.commentId}");
            }
            catch (Exception exception)
            {
                connection.Close();
                return StatusCode(500, exception.Message);
            }
        }
    }
}
