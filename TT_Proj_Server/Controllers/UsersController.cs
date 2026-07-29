using TT_Proj_Server.DAL;
using TT_Proj_Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace TT_Proj_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UsersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("Register")]
        public IActionResult Register([FromBody] User user)
        {
            try
            {
                DBServices db = new DBServices(_configuration);
                int rows = db.InsertUser(user);

                if (rows != 0)
                {
                    return Ok(new { message = "ההרשמה הצליחה!" });
                }
                return BadRequest("אירעה שגיאה בשמירה למסד הנתונים.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            DBServices db = new DBServices(_configuration);
            User user = db.Login(request.Email, request.Password);

            if (user != null)
            {
                db.LogUserLogin(user.Email);

                HttpContext.Session.SetString("Email", user.Email);
                HttpContext.Session.SetString("FullName", user.FirstName + " " + user.LastName);
                HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());
                HttpContext.Session.SetString("IsLocked", user.IsLocked.ToString());
                HttpContext.Session.SetString("IsMaster", user.IsMaster.ToString());

                return Ok(new { message = "התחברת בהצלחה" });
            }
            return BadRequest("אימייל או סיסמה שגויים.");
        }

        [HttpGet("CheckSession")]
        public IActionResult CheckSession()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized("משתמש לא מחובר");
            }

            return Ok(new
            {
                Email = email,
                FullName = HttpContext.Session.GetString("FullName"),
                IsAdmin = HttpContext.Session.GetString("IsAdmin") == "True",
                IsLocked = HttpContext.Session.GetString("IsLocked") == "True",
                IsMaster = HttpContext.Session.GetString("IsMaster") == "True"
            });
        }

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); 
            return Ok();
        }

        public class AddPointsRequest
        {
            public string Email { get; set; }
            public int PointsToAdd { get; set; }
        }

        [HttpPut("AddPoints")]
        public IActionResult AddPoints([FromBody] AddPointsRequest request)
        {
            DBServices db = new DBServices(_configuration);
            bool isUpdated = db.AddPointsToUser(request.Email, request.PointsToAdd);
            if (isUpdated) return Ok();
            return BadRequest("שגיאה בעדכון הנקודות");
        }

        public class UpdateProfileDto
        {
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Password { get; set; }
        }

        [HttpPut("UpdateProfile")]
        public IActionResult UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.Email))
                {
                    return BadRequest("חסר אימייל לביצוע העדכון.");
                }

                User updatedUser = new User
                {
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Password = dto.Password
                };

                DBServices db = new DBServices(_configuration);
                bool isUpdated = db.UpdateUserProfile(updatedUser);

                if (isUpdated)
                {
                    return Ok(new { message = "הפרופיל עודכן בהצלחה!" });
                }
                return BadRequest("שגיאה בעדכון - ייתכן שהמשתמש אינו קיים.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class HighScoreRequest
        {
            public string Email { get; set; }
            public string QuizType { get; set; }
            public int Score { get; set; }
        }

        [HttpPost("UpdateHighScore")]
        public IActionResult UpdateHighScore([FromBody] HighScoreRequest request)
        {
            try
            {
                DBServices db = new DBServices(_configuration);
                db.UpdateHighScore(request.Email, request.QuizType, request.Score);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetLeaderboard/{quizType}")]
        public IActionResult GetLeaderboard(string quizType)
        {
            try
            {
                DBServices db = new DBServices(_configuration);
                List<DBServices.LeaderboardEntry> leaders = db.GetTop10Scores(quizType);
                return Ok(leaders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        public class UserListsRequest
        {
            public string Email { get; set; }
            public Dictionary<string, List<string>> Lists { get; set; }
        }

        [HttpGet("GetUserLists/{email}")]
        public IActionResult GetUserLists(string email)
        {
            try
            {
                DBServices db = new DBServices(_configuration);
                var lists = db.GetUserLists(email);
                return Ok(lists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("SaveUserLists")]
        public IActionResult SaveUserLists([FromBody] UserListsRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || request.Lists == null)
                {
                    return BadRequest("נתונים חסרים");
                }

                DBServices db = new DBServices(_configuration);
                db.SaveUserLists(request.Email, request.Lists);

                return Ok(new { message = "הרשימות נשמרו בהצלחה במסד הנתונים" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}