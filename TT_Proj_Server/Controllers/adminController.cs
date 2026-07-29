using Microsoft.AspNetCore.Mvc;
using TT_Proj_Server.Models;
using TT_Proj_Server.DAL;

namespace TT_Proj_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AdminController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("GetUsers")]
        public IActionResult GetUsers()
        {
            DBServices db = new DBServices(_configuration);
            return Ok(db.GetAllUsersForAdmin());
        }

        [HttpPut("ToggleLock/{email}/{isLocked}")]
        public IActionResult ToggleLock(string email, bool isLocked)
        {
            DBServices db = new DBServices(_configuration);
            bool result = db.UpdateUserLockStatus(email, isLocked);
            if (result) return Ok();
            return BadRequest("שגיאה בעדכון הסטטוס");
        }

        [HttpPut("ToggleAdmin/{email}/{isAdmin}")]
        public IActionResult ToggleAdmin(string email, bool isAdmin)
        {
            DBServices db = new DBServices(_configuration);
            bool result = db.UpdateUserAdminStatus(email, isAdmin);
            if (result) return Ok();
            return BadRequest("שגיאה בעדכון הרשאות המנהל - ייתכן והאימייל לא נמצא במסד הנתונים.");
        }

        [HttpGet("GetStats")]
        public IActionResult GetStats()
        {
            DBServices db = new DBServices(_configuration);
            return Ok(db.GetAdminStatistics());
        }
    }
}