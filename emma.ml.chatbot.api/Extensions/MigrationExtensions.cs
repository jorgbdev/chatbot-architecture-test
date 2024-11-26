using emma.ml.chatbot.api.Database;
using Microsoft.EntityFrameworkCore;

namespace emma.ml.chatbot.api.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILogger<WebApplication>>();

        try
        {
            // Apply pending migrations
            dbContext.Database.Migrate();
            logger.LogInformation("Database migration applied successfully.");

            // Seed the database with initial data
            SeedDatabase(dbContext, app.Configuration, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        }
    }

    private static void SeedDatabase(
        ApplicationDbContext dbContext,
        IConfiguration configuration,
        ILogger logger
    )
    {
    }
}
