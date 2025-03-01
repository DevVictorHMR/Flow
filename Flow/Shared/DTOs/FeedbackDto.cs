namespace Flow.Shared.Dtos
{
    public class FeedbackDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string SenderName { get; set; } = string.Empty;
    }
}