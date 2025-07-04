using Flow.Core.Entities;
using Flow.Core.Interfaces;
using Flow.Infrastructure.Data;
using Flow.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Flow.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}/badges")]
        public async Task<IActionResult> GetUserBadges(string userId)
        {
            var badges = await _context.Users
                .Where(u => u.Id == userId)
                .SelectMany(u => u.Badges)
                .Select(b => new BadgeDto
                {
                    Name = b.Name,
                    IconUrl = b.IconUrl
                })
                .ToListAsync();

            return Ok(badges);
        }
    }
}