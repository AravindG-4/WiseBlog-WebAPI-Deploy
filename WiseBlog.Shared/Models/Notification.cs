using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace WiseBlog.Shared.Models
{
    public class Notification
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
        [BsonElement("Description")]
        public string description { get; set; }

        [Required]
        [BsonElement("Redirection_Link")]
        public string redirectTo { get; set; }

        [Required]
        [BsonElement("CreatedAt")]
        public DateTime created_at { get; set; } = DateTime.UtcNow;

    }

}
