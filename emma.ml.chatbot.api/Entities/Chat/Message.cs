namespace emma.ml.chatbot.api.Entities.Chat;

public class Message
{
    public int Id { get; set; }
    public Guid ConversationId { get; set; }
    public int ParticipantId { get; set; }
    public required string MessageText { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "sent"; // 'sent', 'received', 'delivered', 'read'
    public Conversation? Conversation { get; set; }
    public Participant? Participant { get; set; }
}