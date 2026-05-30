using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inventory.Data;
using Inventory.Models;
using System.Security.Claims;

namespace Inventory.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly PostRepository _repository;

        public PostsController(PostRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
        {
            var posts = await _repository.GetAllAsync();
            return Ok(posts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(int id)
        {
            var post = await _repository.GetByIdAsync(id);
            if (post == null) return NotFound();
            return Ok(post);
        }

        [HttpPost]
        public async Task<ActionResult<Post>> CreatePost(Post post)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            post.AuthorId = userId;
            
            var createdPost = await _repository.CreateAsync(post);
            return CreatedAtAction(nameof(GetPost), new { id = createdPost.PostID }, createdPost);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, Post post)
        {
            if (id != post.PostID) return BadRequest();
            
            var existingPost = await _repository.GetByIdAsync(id);
            if (existingPost == null) return NotFound();

            // Check if user is author
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (existingPost.AuthorId != userId) return Forbid();

            existingPost.Title = post.Title;
            existingPost.Content = post.Content;
            
            await _repository.UpdateAsync(existingPost);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var existingPost = await _repository.GetByIdAsync(id);
            if (existingPost == null) return NotFound();

            // Check if user is author
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (existingPost.AuthorId != userId) return Forbid();

            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}
