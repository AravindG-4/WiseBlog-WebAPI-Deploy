using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;


namespace WiseBlog.Shared.Models
{
    public class Blog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [Required]
        [BsonElement("userID")]
        public string userId { get; set; }

        [Required]
        [BsonElement("userName")]
        public string userName { get; set; }

        [Required]
        [BsonElement("Title")]
        public string title { get; set; }

        [Required]
        [BsonElement("Description")]
        public string description { get; set; }

        [Required]
        [BsonElement("Category")]
        public BlogCategory category { get; set; }

        [Required]
        [BsonElement("Visibility")]
        public VisibilityOptions visibility { get; set; }

        [Required]
        [BsonElement("ContentID")]
        public string contentId { get; set; }

        [Required]
        [BsonElement("CreatedAt")]
        public DateTime created_at { get; set; } = DateTime.UtcNow;

    }

    public enum VisibilityOptions
    {
        Public,
        Private,
        Followers
    }

    public enum BlogCategory
    {
        Technology,
        Lifestyle,
        HealthCare
    }

    public class FollowRequest
    {
        public string UserId { get; set; }
        public string BlogUserId { get; set; }
        public bool ToFollow { get; set; }
    }

}
