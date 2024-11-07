using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDoDienTu.Data;
using WebDoDienTu.Models;
using WebDoDienTu.Models.Repository;
using WebDoDienTu.Models.ViewModels;

namespace WebDoDienTu.Controllers
{
    public class PostsController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;

        public PostsController(IPostRepository postRepository, ICommentRepository commentRepository)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _postRepository.GetAllPostsAsync();
            return View(posts);
        }

        public async Task<IActionResult> Details(int id)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null) return NotFound();

            var comments = await _commentRepository.GetCommentsByPostIdAsync(id);

            var viewModel = new PostDetailsViewModel
            {
                Post = post,
                Comments = comments
            };

            return View(viewModel);
        }
    }
}
