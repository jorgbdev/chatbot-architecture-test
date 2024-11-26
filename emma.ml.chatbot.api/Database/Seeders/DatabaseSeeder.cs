using Microsoft.EntityFrameworkCore;

namespace emma.ml.chatbot.api.Database.Seeders;

public class DatabaseSeeder
{
    private readonly IEnumerable<ISeeder> _seeders;

    public DatabaseSeeder(IEnumerable<ISeeder> seeders)
    {
        _seeders = seeders;
    }

    public async Task SeedAsync(ApplicationDbContext dbContext)
    {
        foreach (var seeder in _seeders)
        {
            // Check if the seeder has already been executed
            if (!await dbContext.SeederHistories.AnyAsync(sh => sh.SeederName == seeder.Name))
            {
                await seeder.SeedAsync(dbContext);
            }
        }
    }
}