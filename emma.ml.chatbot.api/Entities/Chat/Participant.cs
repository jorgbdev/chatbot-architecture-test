namespace emma.ml.chatbot.api.Entities.Chat;

public class Participant
{
    public int Id { get; set; }
    public required string Role { get; set; } // 'client', 'representative', 'bot'
    public required string UserId { get; set; } // Unique user identifier
    public ICollection<ConversationParticipant> ConversationParticipants { get; set; } = [];
    public ICollection<Message> Messages { get; set; } = [];
}