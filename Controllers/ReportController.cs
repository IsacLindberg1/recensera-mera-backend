using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace RecenseraMera_API.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class ReportController : ControllerBase
    {
        MySqlConnection connection;

        public ReportController(IConfiguration config)
        {
            string ip = config["ip"];
            connection = new MySqlConnection(ip);
        }

        [HttpPost("ReportReview")]
        public ActionResult ReportReview(Report report)
        {
            try
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.Prepare();
                command.CommandText = "INSERT INTO `reports` (`reviewId`, `userId`) VALUES (@reviewId, @userId)";
                command.Parameters.AddWithValue("@reviewId", report.reviewId);
                command.Parameters.AddWithValue("@userId", report.userId);
                command.ExecuteNonQuery();

                connection.Close();
                return StatusCode(200, $"Successfully reported review with reviewId: {report.reviewId}");
            }
            catch (Exception exception)
            {
                connection.Close();
                return StatusCode(500, exception.Message);
            }
        }
    }
}