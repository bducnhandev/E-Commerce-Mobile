namespace WebDoDienTu.Models.ViewModels
{
    public class PostDetailsViewModel
    {
        public Post Post { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
        public Comment NewComment { get; set; } = new Comment();
    }
}
