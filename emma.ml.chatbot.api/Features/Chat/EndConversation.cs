using emma.ml.chatbot.api.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace emma.ml.chatbot.api.Features.Chat;

public class EndConversationRequest
{
    public required Guid ConversationId { get; set; }
}

public class EndConversationResponse
{
    public string Message { get; set; } = "Conversation ended successfully.";
}

public class EndConversationEndpoint : Endpoint<EndConversationRequest, EndConversationResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public EndConversationEndpoint(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("/api/chat/conversation/end");
        AllowAnonymous();
        Options(x => x.WithTags("Chat"));
    }

    public override async Task HandleAsync(EndConversationRequest req, CancellationToken ct)
    {
        var conversation = await _dbContext.Conversations
            .FirstOrDefaultAsync(c => c.Id == req.ConversationId, ct);

        if (conversation == null)
        {
            await SendAsync(new EndConversationResponse { Message = "Conversation not found." }, statusCode: 404, cancellation: ct);
            return;
        }

        conversation.Status = "ended";
        _dbContext.Conversations.Update(conversation);
        await _dbContext.SaveChangesAsync(ct);

        await SendAsync(new EndConversationResponse(), cancellation: ct);
    }
}
