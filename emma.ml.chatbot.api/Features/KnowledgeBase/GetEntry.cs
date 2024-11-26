using emma.ml.chatbot.api.Database;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace emma.ml.chatbot.api.Features.KnowledgeBase;

public class GetEntryRequest
{
    public required string Theme { get; set; }
    public required string Topic { get; set; }
    public required string Key { get; set; }
}

public class GetEntryResponse
{
    public required string Value { get; set; }
}

public class GetEntryValidator : Validator<GetEntryRequest>
{
    public GetEntryValidator()
    {
        RuleFor(x => x.Theme)
            .NotEmpty().WithMessage("Theme is required.")
            .MaximumLength(100).WithMessage("Theme cannot exceed 100 characters.");

        RuleFor(x => x.Topic)
            .NotEmpty().WithMessage("Topic is required.")
            .MaximumLength(100).WithMessage("Topic cannot exceed 100 characters.");

        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key is required.")
            .MaximumLength(100).WithMessage("Key cannot exceed 100 characters.");
    }
}

public class GetEntryEndpoint : Endpoint<GetEntryRequest, GetEntryResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetEntryEndpoint(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("/api/entries");
        AllowAnonymous();
        Options(x => x.WithTags("Entries"));
    }

    public override async Task HandleAsync(GetEntryRequest req, CancellationToken ct)
    {
        var theme = await _dbContext.Themes
            .Include(t => t.Topics)
                .ThenInclude(tp => tp.Entries)
            .FirstOrDefaultAsync(t => t.Name.Equals(req.Theme, StringComparison.OrdinalIgnoreCase), ct);

        if (theme == null)
        {
            ThrowError("Theme not found.", statusCode: 404);
        }

        var topic = theme.Topics.FirstOrDefault(tp => tp.Name.Equals(req.Topic, StringComparison.OrdinalIgnoreCase));

        if (topic == null)
        {
            ThrowError("Topic not found under the specified theme.", statusCode: 404);
        }

        var entryData = topic.Entries.FirstOrDefault(e => e.Key.Equals(req.Key, StringComparison.OrdinalIgnoreCase));

        if (entryData == null)
        {
            ThrowError("Entry not found for the specified topic.", statusCode: 404);
        }

        var response = new GetEntryResponse
        {
            Value = entryData.Value
        };

        await SendAsync(response, cancellation: ct);
    }
}