namespace emma.ml.chatbot.api.Entities;

public class SeederHistory
{
    public int Id { get; set; }
    public required string SeederName { get; set; }
    public DateTime ExecutedAt { get; set; }
}