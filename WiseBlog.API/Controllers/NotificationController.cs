using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using WiseBlog.Services;
using WiseBlog.Shared.Models;
using System.Text.Json;

namespace WiseBlog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly MongoDBService _mongoDBService;

        public NotificationController(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        [HttpGet("GetNotifications/{userid}")]
        public async Task<IActionResult> GetNotification(string userid)
        {
            var notifications = await _mongoDBService.GetNotifications(userid);
            return Ok(notifications);
        }
    }
}
