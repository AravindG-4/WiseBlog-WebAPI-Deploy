using Microsoft.AspNetCore.Mvc;
using WiseBlog.Services;
using WiseBlog.Shared.Models;

namespace WiseBlog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly MongoDBService _mongoDBService;

        public ProfileController(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        [HttpPost(Name = "RegisterProfile")]
        public async Task<IActionResult> RegisterProfileAsync([FromBody] Profile request)
        {
            Console.WriteLine("API HITTING WITH THE URL, API WORKING HEEYYYYYYYY");
            Profile RegisteredProfile = await _mongoDBService.RegisterProfile(request);
            return Ok(RegisteredProfile);
        }
        
        [HttpGet(Name = "GetProfileById")]
        public async Task<IActionResult> GetProfileAsync(string userId)
        {
            Profile profile = await _mongoDBService.GetProfile(userId);
            Console.WriteLine($"Profile Got: {profile.name}");
            return Ok(profile);
        }

        [HttpGet("GetProfileImage/{userId}")]
        public async Task<IActionResult> GetProfileImage(string userId)
        {
            var imageString = await _mongoDBService.GetProfileImage(userId);
            return Ok(imageString);
        }

        [HttpPost("ToggleFollow")]
        public async Task<IActionResult> ToggleFollow([FromBody] FollowRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.BlogUserId))
            {
                return BadRequest("Invalid user data.");
            }

            try
            {
                string FollowRequestResponse = await _mongoDBService.ToggleFollow(request.UserId, request.BlogUserId, request.ToFollow);
                return Ok(FollowRequestResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating follow status: {ex.Message}");
            }
        }
    }
}
