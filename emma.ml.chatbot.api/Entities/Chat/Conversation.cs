namespace emma.ml.chatbot.api.Entities.Chat;

public class Conversation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public string Status { get; set; } = "awaiting_representative"; // 'awaiting_representative', 'active', 'paused', 'ended'
    public int? CurrentRepresentativeId { get; set; }
    public Participant? CurrentRepresentative { get; set; }

    public ICollection<ConversationParticipant> ConversationParticipants { get; set; } = [];
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}