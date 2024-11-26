using emma.ml.chatbot.api.Database;
using emma.ml.chatbot.api.Entities.KnowledgeBase;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace emma.ml.chatbot.api.Features.KnowledgeBase;

public class CreateThemeRequest
{
    public required string Name { get; set; }
}

public class CreateThemeResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
}

public class CreateThemeValidator : Validator<CreateThemeRequest>
{
    public CreateThemeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Theme name is required.")
            .MaximumLength(100).WithMessage("Theme name cannot exceed 100 characters.");
    }
}

public class CreateThemeEndpoint : Endpoint<CreateThemeRequest, CreateThemeResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateThemeEndpoint(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("/api/themes");
        AllowAnonymous();
        Options(x => x.WithTags("Themes"));
    }

    public override async Task HandleAsync(CreateThemeRequest req, CancellationToken ct)
    {
        var existingTheme = await _dbContext.Themes
            .FirstOrDefaultAsync(t => t.Name.Equals(req.Name, StringComparison.CurrentCultureIgnoreCase), ct);

        if (existingTheme != null)
        {
            AddError(e => e.Name, "A theme with this name already exists.");
            ThrowIfAnyErrors();
        }

        var theme = new Theme
        {
            Name = req.Name
        };

        _dbContext.Themes.Add(theme);
        await _dbContext.SaveChangesAsync(ct);

        var response = new CreateThemeResponse
        {
            Id = theme.Id,
            Name = theme.Name
        };

        await SendAsync(response, statusCode: 201, cancellation: ct);
    }
}