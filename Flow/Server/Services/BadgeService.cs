using Flow.Infrastructure.Data;
using Flow.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Data.Entity;

public class BadgeService
{
    private readonly AppDbContext _context;
    private readonly ILogger<BadgeService> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;

    public BadgeService(
        AppDbContext context,
        ILogger<BadgeService> logger,
        IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task CheckAndAssignBadgesAsync(string userId)
    {
        var user = await _context.Users
            .Include(u => u.Badges)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return;

        var totalFlowbits = await _context.Feedbacks
            .Where(f => f.ReceiverId == userId)
            .SumAsync(f => f.Flowbits);

        var badgesToAdd = await _context.Badges
            .Where(b => b.FlowbitsThreshold <= totalFlowbits && !user.Badges.Contains(b))
            .ToListAsync();

        if (badgesToAdd.Any())
        {
            user.Badges.AddRange(badgesToAdd);
            await _context.SaveChangesAsync();

            foreach (var badge in badgesToAdd)
            {
                await _hubContext.Clients.Group(userId)
                    .SendAsync("BadgeEarned", new { BadgeName = badge.Name });
            }
        }
    }
}