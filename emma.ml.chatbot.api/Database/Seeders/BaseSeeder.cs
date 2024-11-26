using emma.ml.chatbot.api.Entities;
using Microsoft.EntityFrameworkCore;

namespace emma.ml.chatbot.api.Database.Seeders;

public abstract class BaseSeeder : ISeeder
{
    public abstract string Name { get; }

    public async Task SeedAsync(ApplicationDbContext dbContext)
    {
        // Begin a transaction to ensure atomicity
        using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            if (await HasSeederRunAsync(dbContext))
            {
                // Seeder has already run; skip
                return;
            }

            // Execute the seeder-specific logic
            await SeedDataAsync(dbContext);

            // Record the seeder execution
            var seederHistory = new SeederHistory
            {
                SeederName = this.Name,
                ExecutedAt = DateTime.UtcNow
            };

            dbContext.SeederHistories.Add(seederHistory);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            var message = ex.Message;
            await transaction.RollbackAsync();
        }
    }

    protected abstract Task SeedDataAsync(ApplicationDbContext dbContext);

    private async Task<bool> HasSeederRunAsync(ApplicationDbContext dbContext)
    {
        return await dbContext.SeederHistories.AnyAsync(sh => sh.SeederName == this.Name);
    }
}