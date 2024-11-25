using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebDoDienTu.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Content { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsPublished { get; set; }

        public string? AuthorId { get; set; }
        public int CategoryId { get; set; }
        public virtual ApplicationUser? Author { get; set; }
        public virtual PostCategory? Category { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; }
        public virtual ICollection<ActionPost>? ActionPosts {  get; set; }    
    }

}
