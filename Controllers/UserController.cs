using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Crypto.Generators;
using System.Collections;
using System.Text;

namespace RecenseraMera_API.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class UserController : Controller
    {
        MySqlConnection connection;

        public UserController(IConfiguration config)
        {
            string ip = config["ip"];
            connection = new MySqlConnection(ip);
        }

        public static Hashtable sessionId = new Hashtable();

        [HttpPost("CreateUser")]
        public ActionResult createAccount(User user)
        {
            string checkUniqueUser = CheckIfUniqueUserDataExists(user);
            if (checkUniqueUser != String.Empty)
            {
                return BadRequest(checkUniqueUser);
            }

            try
            {
                connection.Open();
                string authorization = Request.Headers["Authorization"];

                MySqlCommand command = connection.CreateCommand();
                command.Prepare();

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.password);
                command.CommandText = "INSERT INTO `user` (`role`, `username`, `password`, `email`) VALUES ('2', @username, @password, @email)";
                command.Parameters.AddWithValue("@username", user.username);
                command.Parameters.AddWithValue("@password", hashedPassword);
                command.Parameters.AddWithValue("@email", user.email);

                int rows = command.ExecuteNonQuery();

                if (rows > 0)
                {
                    Guid guid = Guid.NewGuid();
                    string key = guid.ToString();
                    user.userId = (int)command.LastInsertedId;
                    sessionId.Add(key, user);
                    connection.Close();
                    return StatusCode(201, key);
                }
            }
            catch (Exception exception)
            {
                connection.Close();
                return StatusCode(500, exception.Message);
            }
            return StatusCode(400);
        }

        [HttpGet("Login")] 
        public ActionResult Login() 
        {
            string auth = this.HttpContext.Request.Headers["Authorization"];
            User user = DecodeUser(new User(), auth);
            connection.Open();
            MySqlCommand command = connection.CreateCommand();
            command.Prepare();
            command.CommandText = "SELECT * FROM user WHERE username = @username";
            command.Parameters.AddWithValue("@username", user.username);
            MySqlDataReader data = command.ExecuteReader();
            try
            {
                string passwordHash = String.Empty;

                while (data.Read())
                {       
                    user.userId = data.GetInt32("userId");
                    user.username = data.GetString("username");
                    passwordHash = data.GetString("password");
                    user.role = data.GetInt32("role");
                }

                if (passwordHash != string.Empty && BCrypt.Net.BCrypt.Verify(user.password, passwordHash))
                {
                    Guid guid = Guid.NewGuid();
                    string key = guid.ToString();
                    Console.WriteLine(key);
                    sessionId.Add(key, user);
                    connection.Close();
                    return Ok(key);
                }

                connection.Close();
                return StatusCode(400);
            }
            catch (Exception exception)
            {
                connection.Close();
                Console.WriteLine($"Login failed: {exception.Message}");
                return StatusCode(500);
            }
        }

        [HttpGet("VerifyRole")]
        public ActionResult VerifyRole()
        {
            string auth = this.HttpContext.Request.Headers["Authorization"];

            if (auth == null || !UserController.sessionId.ContainsKey(auth))
            {
                return StatusCode(403, "0");
            }
            User user = (User)UserController.sessionId[auth];
            return StatusCode(200, user.role);
        }

        [HttpGet("VerifyUserId")]
        public ActionResult VerifyUserId()
        {
            string auth = this.HttpContext.Request.Headers["Authorization"];
            if (auth == null || !UserController.sessionId.ContainsKey(auth))
            {
                return StatusCode(403, "0");
            }

            User user = (User)UserController.sessionId[auth];
            return StatusCode(200, user.userId);
        }

        private string CheckIfUniqueUserDataExists(User user)
        {
            string checkUniqueUser = String.Empty;
            try
            {
                MySqlCommand query = connection.CreateCommand();
                query.Prepare();
                query.CommandText = "SELECT * FROM user WHERE username = @username";
                query.Parameters.AddWithValue("@username", user.username);
                MySqlDataReader data = query.ExecuteReader();

                if (data.Read())
                {
                    if (data.GetString("username") == user.username)
                    {
                        checkUniqueUser = "Användarnamn används redan på hemsidan";
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"UserController.CheckIfUniqueUserDataExists: {exception.Message}");
                connection.Close();
            }

            return checkUniqueUser;
        }

        private User DecodeUser(User user, string auth)
        {
            if (auth != null && auth.StartsWith("Basic"))
            {
                string encodedUsernamePassword = auth.Substring("Basic ".Length).Trim();
                Encoding encoding = Encoding.GetEncoding("UTF-8");
                string usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));
                int seperatorIndex = usernamePassword.IndexOf(':');
                user.username = usernamePassword.Substring(0, seperatorIndex);
                user.password = usernamePassword.Substring(seperatorIndex + 1);
            }
            else
            {
                throw new Exception("The authorization header is either empty or isn't Basic.");
            }
            return user;
        }
    }
}
