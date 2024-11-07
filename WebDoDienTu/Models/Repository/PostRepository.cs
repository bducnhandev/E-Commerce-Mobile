using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebDoDienTu.Data;

namespace WebDoDienTu.Models.Repository
{
    public class PostRepository : IPostRepository
    {
        private readonly ApplicationDbContext _context;

        public PostRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Post>> GetAllPostsAsync()
        {
            return await _context.Posts.Include(p => p.Category).ToListAsync();
        }

        public async Task<Post> GetPostByIdAsync(int id)
        {
            return await _context.Posts.Include(p => p.Category).FirstOrDefaultAsync(p => p.PostId == id);
        }

        public async Task AddPostAsync(Post post)
        {
            post.CreatedAt = DateTime.Now;
            post.UpdatedAt = DateTime.Now;
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePostAsync(Post post)
        {
            post.UpdatedAt = DateTime.Now;
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePostAsync(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }
        }
    }
}
