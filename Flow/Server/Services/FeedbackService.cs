using Flow.Core.Entities;
using Flow.Core.Interfaces;
using Flow.Infrastructure.Data;
using Flow.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Flow.Server.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly AppDbContext _context;

        public FeedbackService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateFeedbackAsync(FeedbackCreateDto feedbackDto, string senderId)
        {
            var feedback = new Feedback
            {
                Content = feedbackDto.Content,
                SenderId = senderId,
                ReceiverId = feedbackDto.ReceiverId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Feedbacks.AddAsync(feedback);
            await _context.SaveChangesAsync();
        }

        public async Task<List<FeedbackDto>> GetFeedbacksForUserAsync(string userId)
        {
            return await _context.Feedbacks
                .Where(f => f.ReceiverId == userId)
                .Include(f => f.Sender)
                .Select(f => new FeedbackDto
                {
                    Id = f.Id,
                    Content = f.Content,
                    CreatedAt = f.CreatedAt,
                    SenderName = f.Sender.FullName
                })
                .ToListAsync();
        }
    }
}