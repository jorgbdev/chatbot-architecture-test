namespace emma.ml.chatbot.api.Entities.Chat;

public class ConversationParticipant
{
    public Guid ConversationId { get; set; }
    public Conversation? Conversation { get; set; }

    public int ParticipantId { get; set; }
    public Participant? Participant { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
    public string Status { get; set; } = "available"; // 'available', 'busy'
}