using Flow.Shared.Dtos;
using XAct.Messages;

namespace Flow.Core.Interfaces
{
    public interface IFeedbackService
    {
        Task CreateFeedbackAsync(FeedbackCreateDto feedbackDto, string senderId);
        Task<ApiPagedResponse<FeedbackDto>> GetFeedbacksForUserAsync(string userId, int pageNumber = 1, int pageSize = 10);
        Task AddFlowbitsAsync(FlowbitsDto flowbitsDto);
    }
}