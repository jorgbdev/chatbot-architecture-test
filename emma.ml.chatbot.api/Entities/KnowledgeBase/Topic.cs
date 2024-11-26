namespace emma.ml.chatbot.api.Entities.KnowledgeBase;

public class Topic
{
    public int Id { get; set; }
    public int ThemeId { get; set; }
    public required string Name { get; set; }

    // Navigation Properties
    public Theme? Theme { get; set; }
    public ICollection<Entry> Entries { get; set; } = [];
}