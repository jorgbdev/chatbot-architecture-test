using emma.ml.chatbot.api.Entities;
using emma.ml.chatbot.api.Entities.KnowledgeBase;
using Microsoft.EntityFrameworkCore;

namespace emma.ml.chatbot.api.Database.Seeders.Features;

public class PBIKnowledgeBaseFirstTest : BaseSeeder
{
    public override string Name => "PBIKnowledgeBaseFirstTest";

    protected override async Task SeedDataAsync(ApplicationDbContext dbContext)
    {
        // 1. Seed Themes
        var themes = new[] { "University", "Business Objective", "Product Information", "FAQ" };
        foreach (var themeName in themes)
        {
            if (!await dbContext.Themes.AnyAsync(t => t.Name == themeName))
            {
                dbContext.Themes.Add(new Theme { Name = themeName });
            }
        }
        await dbContext.SaveChangesAsync();

        // 2. Seed Topics
        // University Topics
        var universityTheme = await dbContext.Themes.FirstOrDefaultAsync(t => t.Name == "University");
        if (universityTheme != null)
        {
            var universityTopics = new[] { "Harvard University", "Stanford University" };
            foreach (var topicName in universityTopics)
            {
                if (!await dbContext.Topics.AnyAsync(tp => tp.Name == topicName && tp.ThemeId == universityTheme.Id))
                {
                    dbContext.Topics.Add(new Topic { Name = topicName, ThemeId = universityTheme.Id });
                }
            }
        }

        // Business Objective Topics
        var businessObjectiveTheme = await dbContext.Themes.FirstOrDefaultAsync(t => t.Name == "Business Objective");
        if (businessObjectiveTheme != null)
        {
            var businessTopics = new[] { "Q3 Revenue Goals", "Market Expansion" };
            foreach (var topicName in businessTopics)
            {
                if (!await dbContext.Topics.AnyAsync(tp => tp.Name == topicName && tp.ThemeId == businessObjectiveTheme.Id))
                {
                    dbContext.Topics.Add(new Topic { Name = topicName, ThemeId = businessObjectiveTheme.Id });
                }
            }
        }
        await dbContext.SaveChangesAsync();

        // 3. Seed Entries
        // University Information for Harvard University
        var harvardTopic = await dbContext.Topics.FirstOrDefaultAsync(tp => tp.Name == "Harvard University");
        if (harvardTopic != null)
        {
            var harvardEntries = new[]
            {
                        new Entry { TopicId = harvardTopic.Id, Key = "location", Value = "Cambridge, MA, USA", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                        new Entry { TopicId = harvardTopic.Id, Key = "plans", Value = "Expand research facilities and increase scholarships.", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                        new Entry { TopicId = harvardTopic.Id, Key = "courses", Value = "Business, Law, Medicine, Engineering", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                        new Entry { TopicId = harvardTopic.Id, Key = "founded_year", Value = "1636", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                        new Entry { TopicId = harvardTopic.Id, Key = "website_url", Value = "https://www.harvard.edu/", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                    };

            foreach (var entry in harvardEntries)
            {
                if (!await dbContext.Entries.AnyAsync(e => e.TopicId == entry.TopicId && e.Key == entry.Key))
                {
                    dbContext.Entries.Add(entry);
                }
            }
        }

        // Business Objective Information for Q3 Revenue Goals
        var q3RevenueGoalsTopic = await dbContext.Topics.FirstOrDefaultAsync(tp => tp.Name == "Q3 Revenue Goals");
        if (q3RevenueGoalsTopic != null)
        {
            if (!await dbContext.Entries.AnyAsync(e => e.TopicId == q3RevenueGoalsTopic.Id && e.Key == "description"))
            {
                dbContext.Entries.Add(new Entry
                {
                    TopicId = q3RevenueGoalsTopic.Id,
                    Key = "description",
                    Value = "Achieve a 15% increase in revenue compared to Q2.",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        // Business Objective Information for Market Expansion
        var marketExpansionTopic = await dbContext.Topics.FirstOrDefaultAsync(tp => tp.Name == "Market Expansion");
        if (marketExpansionTopic != null)
        {
            if (!await dbContext.Entries.AnyAsync(e => e.TopicId == marketExpansionTopic.Id && e.Key == "description"))
            {
                dbContext.Entries.Add(new Entry
                {
                    TopicId = marketExpansionTopic.Id,
                    Key = "description",
                    Value = "Enter the Southeast Asian market by Q4.",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }
        await dbContext.SaveChangesAsync();

        // 4. Record Seeder Execution
        var seederHistory = new SeederHistory
        {
            SeederName = this.Name,
            ExecutedAt = DateTime.UtcNow
        };
        dbContext.SeederHistories.Add(seederHistory);
        await dbContext.SaveChangesAsync();
    }
}
