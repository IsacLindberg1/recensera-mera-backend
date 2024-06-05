using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace RecenseraMera_API.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class CategoryController : Controller
    {
        MySqlConnection connection;

        public CategoryController(IConfiguration config)
        {
            string ip = config["ip"];
            connection = new MySqlConnection(ip);
        }

        [HttpPost("ChooseCategory")]
        public ActionResult ChooseCategory(Category category)
        {
            try
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.Prepare();
                command.CommandText = "UPDATE `review` SET `categoryId` = @categoryId WHERE `reviewId` = @reviewId";
                command.Parameters.AddWithValue("@categoryId", category.categoryId);
                command.Parameters.AddWithValue("@reviewId", category.reviewId);
                int rowsAffected = command.ExecuteNonQuery();

                connection.Close();

                if (rowsAffected > 0)
                {
                    return StatusCode(200, $"Successfully chose category with categoryId: {category.categoryId} for review with reviewId: {category.reviewId}");
                }
                else
                {
                    return StatusCode(404, $"Review with reviewId: {category.reviewId} not found");
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
