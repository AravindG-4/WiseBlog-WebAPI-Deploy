using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace WiseBlog.Shared.Models
{
    public class Profile
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [Required]
        [BsonElement("userID")]
        public string userId { get; set; }

        [Required]
        [BsonElement("Name")]
        public string name { get; set; }

        [Required]
        [BsonElement("Image")]
        public string image { get; set; }

        [Required]
        [BsonElement("Bio")]
        public string bio { get; set; }
        
        [Required]
        [BsonElement("Following")]
        public List<string> following { get; set; }
        
        [Required]
        [BsonElement("Followers")]
        public List<string> followers { get; set; }
        
        
    }
}
