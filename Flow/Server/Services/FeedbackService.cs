using Flow.Core.Entities;
using Flow.Core.Interfaces;
using Flow.Infrastructure.Data;
using Flow.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Flow.Server.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FeedbackService> _logger;
        private readonly BadgeService _badgeService;

        public FeedbackService(AppDbContext context, ILogger<FeedbackService> logger, BadgeService badgeService)
        {
            _context = context;
            _logger = logger;
            _badgeService = badgeService;
        }

        public async Task CreateFeedbackAsync(FeedbackCreateDto feedbackDto, string senderId)
        {
            using var activity = Activity.Current?.Source.StartActivity("CreateFeedback");
            _logger.LogInformation("Iniciando criação de feedback para o usuário {ReceiverId}", feedbackDto.ReceiverId);

            try
            {
                if (senderId == feedbackDto.ReceiverId)
                {
                    _logger.LogWarning("Tentativa de autofeedback do usuário {UserId}", senderId);
                    throw new ArgumentException("Não é possível enviar feedback para si mesmo.");
                }

                var feedback = new Feedback
                {
                    Content = feedbackDto.Content,
                    SenderId = senderId,
                    ReceiverId = feedbackDto.ReceiverId,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Feedbacks.AddAsync(feedback);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Feedback {FeedbackId} criado com sucesso", feedback.Id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Falha ao salvar feedback no banco de dados");
                throw new InvalidOperationException("Erro ao persistir o feedback.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao criar feedback");
                throw;
            }
        }

        public async Task<ApiPagedResponse<FeedbackDto>> GetFeedbacksForUserAsync(string userId, int pageNumber = 1, int pageSize = 10)
        {
            _logger.LogInformation(
                "Buscando feedbacks para o usuário {UserId} (Página {PageNumber}, Tamanho {PageSize})",
                userId, pageNumber, pageSize
            );

            try
            {
                var query = _context.Feedbacks
                    .Where(f => f.ReceiverId == userId)
                    .Include(f => f.Sender)
                    .OrderByDescending(f => f.CreatedAt);

                var totalRecords = await query.CountAsync();
                var feedbacks = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(f => new FeedbackDto
                    {
                        Id = f.Id,
                        Content = f.Content,
                        CreatedAt = f.CreatedAt,
                        SenderName = f.Sender.FullName,
                        Flowbits = f.Flowbits
                    })
                    .ToListAsync();

                _logger.LogDebug("Encontrados {Count} feedbacks", feedbacks.Count);
                return new ApiPagedResponse<FeedbackDto>(feedbacks, pageNumber, pageSize, totalRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao buscar feedbacks para o usuário {UserId}", userId);
                throw new ApplicationException("Erro ao recuperar feedbacks.", ex);
            }
        }

        public async Task AddFlowbitsAsync(FlowbitsDto flowbitsDto)
        {
            var feedback = await _context.Feedbacks.FindAsync(flowbitsDto.FeedbackId);
            if (feedback == null)
            {
                _logger.LogWarning("Feedback {FeedbackId} não encontrado", flowbitsDto.FeedbackId);
                throw new KeyNotFoundException("Feedback não encontrado.");
            }

            feedback.Flowbits += flowbitsDto.Increment;
            await _context.SaveChangesAsync();

            await _badgeService.CheckAndAssignBadgesAsync(feedback.ReceiverId);

            _logger.LogInformation(
                "Flowbits atualizados para {FeedbackId}. Novo total: {Flowbits}",
                feedback.Id, feedback.Flowbits
            );
        }
    }
}