using emma.ml.chatbot.api.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace emma.ml.chatbot.api.Features.KnowledgeBase;

public class GetThemesResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
}

public class GetThemesEndpoint(ApplicationDbContext _dbContext) : EndpointWithoutRequest<List<GetThemesResponse>>
{
    public override void Configure()
    {
        Get("/api/themes");
        AllowAnonymous(); // Adjust as per your authentication requirements
        Options(x => x.WithTags("Themes"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var themes = await _dbContext.Themes
            .OrderBy(t => t.Name)
            .Select(t => new GetThemesResponse
            {
                Id = t.Id,
                Name = t.Name
            })
            .ToListAsync(ct);

        await SendAsync(themes, cancellation: ct);
    }
}