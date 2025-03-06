﻿using Flow.Shared.Dtos;

namespace Flow.Core.Interfaces
{
    public interface IFeedbackService
    {
        Task CreateFeedbackAsync(FeedbackCreateDto feedbackDto, string senderId);
        Task<List<FeedbackDto>> GetFeedbacksForUserAsync(string userId);
    }
}