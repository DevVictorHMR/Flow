using Flow.Core.Interfaces;
using Flow.Server.Services;
using Flow.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flow.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbacksController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbacksController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFeedback([FromBody] FeedbackCreateDto feedbackDto)
        {
            var senderId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            await _feedbackService.CreateFeedbackAsync(feedbackDto, senderId!);
            return Ok();
        }

        [HttpGet("received")]
        public async Task<IActionResult> GetFeedbacks()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var feedbacks = await _feedbackService.GetFeedbacksForUserAsync(userId!);
            return Ok(feedbacks);
        }

    }
}