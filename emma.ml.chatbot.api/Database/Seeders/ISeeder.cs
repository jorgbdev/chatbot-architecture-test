namespace emma.ml.chatbot.api.Database.Seeders;

public interface ISeeder
{
    string Name { get; }
    Task SeedAsync(ApplicationDbContext dbContext);
}