namespace emma.ml.chatbot.api.Entities.KnowledgeBase;

public class Theme
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<Topic> Topics { get; set; } = [];
}