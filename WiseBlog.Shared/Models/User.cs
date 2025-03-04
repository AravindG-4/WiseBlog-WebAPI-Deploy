using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.ComponentModel.DataAnnotations;


namespace WiseBlog.Shared.Models
{
    [Table("Users")]
    public class User : BaseModel
    {
        [PrimaryKey("id", true)]
        [Required]
        public Guid id { get; set; } // Matches int8 in Supabase

        [Column("name")]
        public string name { get; set; } = string.Empty; 

        [Column("email")]
        public string email { get; set; } = string.Empty; 

        [Column("role")]
        public string role { get; set; } = "user"; 

        [Column("created_at")]
        public DateTime created_at { get; set; } = DateTime.UtcNow; 
    }
}