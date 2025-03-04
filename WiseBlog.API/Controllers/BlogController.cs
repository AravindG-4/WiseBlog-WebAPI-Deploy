using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using WiseBlog.Services;
using WiseBlog.Shared.Models;
using System.Text.Json;

namespace WiseBlog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogController : ControllerBase
    {
        private readonly MongoDBService _mongoDBService;

        public BlogController(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        [HttpPost("UploadBlog")]
        public async Task<IActionResult> UploadBlog()
        {
            var file = Request.Form.Files.FirstOrDefault(f => f.FileName == "blogContent.html");
            var metadataFile = Request.Form.Files.FirstOrDefault(f => f.FileName == "metadata.json");

            if (file == null || metadataFile == null || file.Length == 0 || metadataFile.Length == 0)
            {
                return BadRequest("Invalid request. Ensure Title, Description, and Content are provided.");
            }

            string userId, userName, title, description, category, visibility;

            using (var metadataStream = new StreamReader(metadataFile.OpenReadStream()))
            {
                var metadataJson = await metadataStream.ReadToEndAsync();
                var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(metadataJson);

                userId = metadata["userId"];
                userName = metadata["userName"];
                title = metadata["title"];
                description = metadata["description"];
                category = metadata["category"];
                visibility = metadata["visibility"];
            }

            using var contentStream = file.OpenReadStream();
            var memoryStream = new MemoryStream();
            await contentStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            _ = Task.Run(async () =>
            {
                try
                {
                    string blogId = await _mongoDBService.UploadBlogToGridFS(memoryStream, file.ContentType);
                    var blogDocument = new Blog
                    {
                        userId = userId,
                        userName = userName,
                        title = title,
                        description = description,
                        category = Enum.Parse<BlogCategory>(category, true),
                        visibility = Enum.Parse<VisibilityOptions>(visibility, true),
                        contentId = blogId
                    };

                    await _mongoDBService.InsertBlogDocument(blogDocument);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error uploading blog: {ex.Message}");
                    return;
                }
                finally
                {
                    memoryStream.Dispose(); 
                }
            });



            return Ok();
        }

        [HttpPost("SaveSummary")]
        public async Task<IActionResult> SaveSummary([FromBody] Summary summary)
        {
            await _mongoDBService.SaveSummary(summary);
            return Ok();
        }

        [HttpGet("GetSummary/{userid}")]
        public async Task<IActionResult> GetSummary(string userid)
        {
            var summaries = await _mongoDBService.GetSummaries(userid);
            return Ok(summaries);
        }

        [HttpGet("GetBlog/{id}")]
        public async Task<IActionResult> GetBlog(string id)
        {
            var blog = await _mongoDBService.GetBlog(id);
            return Ok(blog);
        }

        [HttpDelete("DeleteBlog/{id}")]
        public async Task<IActionResult> DeleteBlog(string id)
        {
            await _mongoDBService.DeleteBlogDocument(id);
            return Ok();
        }

        [HttpDelete("DeleteSummary/{id}")]
        public async Task<IActionResult> DeleteSummary(string id)
        {
            await _mongoDBService.DeleteSummary(id);
            return Ok();
        }

        [HttpGet("GetContent/{id}")]
        public async Task<IActionResult> GetContent(string id)
        {
            var blogStream = await _mongoDBService.DownloadFileFromGridFS(id);
            using var reader = new StreamReader(blogStream);
            string blogContent = await reader.ReadToEndAsync();

            return Content(blogContent, "text/html");
        }

        [HttpGet("GetAllBlogs")]
        public async Task<IActionResult> GetAllBlogs()
        {
            var blogs = await _mongoDBService.GetAllBlogs();
            return Ok(blogs);
        }
        
        

    }
}
