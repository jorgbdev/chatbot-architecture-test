namespace emma.ml.chatbot.api.Entities.KnowledgeBase;

public class Entry
{
    public int Id { get; set; }
    public int TopicId { get; set; }
    public required string Key { get; set; }
    public required string Value { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Property
    public Topic? Topic { get; set; }
}