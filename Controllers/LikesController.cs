using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace RecenseraMera_API.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class LikesController : ControllerBase
    {
        MySqlConnection connection = new MySqlConnection("server=localhost;uid=root;pwd=;database=recensera-mera");

        [HttpPost("LikeReview")]
        public ActionResult LikeReview(Review like)
        {
            try
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.Prepare();
                command.CommandText = "INSERT INTO `likes` (`reviewId`, `userId`) VALUES (@reviewId, @userId)";
                command.Parameters.AddWithValue("@reviewId", like.reviewId);
                command.Parameters.AddWithValue("@userId", like.userId);
                command.ExecuteNonQuery();

                connection.Close();
                return StatusCode(200, $"Successfully liked review with reviewId: {like.reviewId}");
            }
            catch (Exception exception)
            {
                connection.Close();
                return StatusCode(500, exception.Message);
            }
        }

        [HttpDelete("UnlikeReview")]
        public ActionResult UnlikeReview(int reviewId, int userId)
        {
            try
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.Prepare();
                command.CommandText = "DELETE FROM `likes` WHERE `reviewId` = @reviewId AND `userId` = @userId";
                command.Parameters.AddWithValue("@reviewId", reviewId);
                command.Parameters.AddWithValue("@userId", userId);
                int rowsAffected = command.ExecuteNonQuery();

                connection.Close();

                if (rowsAffected > 0)
                {
                    return StatusCode(200, $"Successfully unliked review with reviewId: {reviewId}");
                }
                else
                {
                    return StatusCode(404, $"Like not found for reviewId: {reviewId} and userId: {userId}");
                }
            }
            catch (Exception exception)
            {
                connection.Close();
                return StatusCode(500, exception.Message);
            }
        }
    }
}
