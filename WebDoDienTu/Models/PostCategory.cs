using System.ComponentModel.DataAnnotations;

namespace WebDoDienTu.Models
{
    public class PostCategory
    {
        [Key]
        public int CategoryId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Post>? Posts { get; set; }
    }
}
