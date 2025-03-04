using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;


namespace WiseBlog.Shared.Models
{
    public class Summary
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [Required]
        [BsonElement("BlogID")]
        public string blogId { get; set; }

        [Required]
        [BsonElement("userID")]
        public string userId { get; set; }

        [Required]
        [BsonElement("userName")]
        public string userName { get; set; }

        [Required]
        [BsonElement("CreatorID")]
        public string CreatorId { get; set; }

        [Required]
        [BsonElement("CreatorName")]
        public string CreatorName { get; set; }

        [Required]
        [BsonElement("Title")]
        public string title { get; set; }

        [Required]
        [BsonElement("Summary")]
        public string summary { get; set; }

        [Required]
        [BsonElement("CreatedAt")]
        public DateTime created_at { get; set; } = DateTime.UtcNow;

    }

}
