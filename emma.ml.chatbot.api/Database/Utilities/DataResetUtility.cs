using Microsoft.EntityFrameworkCore;

namespace emma.ml.chatbot.api.Database.Utilities;

public static class DataResetUtility
{
    public static async Task ClearDatabaseAsync(ApplicationDbContext dbContext)
    {
        // Disable foreign key constraints temporarily
        await dbContext.Database.ExecuteSqlRawAsync("SET session_replication_role = 'replica';");

        // Truncate or delete data from tables in the correct order
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Messages\" RESTART IDENTITY CASCADE;");
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"ConversationParticipants\" RESTART IDENTITY CASCADE;");
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Conversations\" RESTART IDENTITY CASCADE;");
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Participants\" RESTART IDENTITY CASCADE;");
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Entries\" RESTART IDENTITY CASCADE;");
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Topics\" RESTART IDENTITY CASCADE;");
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Themes\" RESTART IDENTITY CASCADE;");
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"SeederHistories\" RESTART IDENTITY CASCADE;");

        // Re-enable foreign key constraints
        await dbContext.Database.ExecuteSqlRawAsync("SET session_replication_role = 'origin';");
    }
}