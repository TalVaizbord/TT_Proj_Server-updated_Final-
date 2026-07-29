using Microsoft.AspNetCore.Mvc;
using TT_Proj_Server.Models;
using TT_Proj_Server.DAL;

namespace TT_Proj_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SharesController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SharesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("GetByCountry/{countryId}")]
        public IActionResult GetByCountry(string countryId)
        {
            DBServices db = new DBServices(_configuration);
            List<Share> shares = db.GetSharesByCountry(countryId);
            return Ok(shares);
        }

        [HttpPost("AddShare")]
        public IActionResult AddShare([FromBody] Share share)
        {
            DBServices db = new DBServices(_configuration);
            int numEffected = db.InsertShare(share);
            if (numEffected > 0) return Ok();
            return BadRequest("Error saving share.");
        }

        [HttpPut("UpdateShare")]
        public IActionResult UpdateShare([FromBody] Share share)
        {
            DBServices db = new DBServices(_configuration);
            bool isUpdated = db.UpdateShare(share.Id, share.Content);
            if (isUpdated) return Ok();
            return BadRequest("Error updating share.");
        }

        [HttpDelete("DeleteShare/{id}")]
        public IActionResult DeleteShare(int id)
        {
            DBServices db = new DBServices(_configuration);
            bool isDeleted = db.DeleteShare(id);
            if (isDeleted) return Ok();
            return BadRequest("Error deleting share.");
        }
    }
}