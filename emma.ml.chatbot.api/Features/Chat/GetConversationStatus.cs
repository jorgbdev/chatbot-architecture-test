using emma.ml.chatbot.api.Database;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace emma.ml.chatbot.api.Features.Chat;

public class GetConversationStatusRequest
{
    public required Guid ConversationId { get; set; }
}

public class GetConversationStatusResponse
{
    public string Status { get; set; } = "active"; // 'awaiting_representative', 'active', 'paused', 'ended'
}

public class GetConversationStatusValidator : Validator<GetConversationStatusRequest>
{
    public GetConversationStatusValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("ConversationId is required.");
    }
}

public class GetConversationStatusEndpoint : Endpoint<GetConversationStatusRequest, GetConversationStatusResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetConversationStatusEndpoint(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("/api/chat/status");
        AllowAnonymous();
        Options(x => x.WithTags("Chat"));
    }

    public override async Task HandleAsync(GetConversationStatusRequest req, CancellationToken ct)
    {
        var conversation = await _dbContext.Conversations
            .FirstOrDefaultAsync(c => c.Id == req.ConversationId, ct);

        if (conversation == null)
        {
            await SendNotFoundAsync(cancellation: ct);
            return;
        }

        var response = new GetConversationStatusResponse
        {
            Status = conversation.Status
        };

        await SendAsync(response, cancellation: ct);
    }
}