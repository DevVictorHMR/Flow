namespace Flow.Core.Entities
{
    public class Feedback
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string SenderId { get; set; } = string.Empty;
        public User Sender { get; set; } = null!;

        public string ReceiverId { get; set; } = string.Empty;
        public User Receiver { get; set; } = null!;
    }
}