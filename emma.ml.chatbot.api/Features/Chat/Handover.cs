using emma.ml.chatbot.api.Database;
using emma.ml.chatbot.api.Entities.Chat;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace emma.ml.chatbot.api.Features.Chat;

public class HandoverRequest
{
    public required Guid ConversationId { get; set; }
}

public class HandoverResponse
{
    public int RepresentativeId { get; set; }
}

public class HandoverEndpoint : Endpoint<HandoverRequest, HandoverResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public HandoverEndpoint(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("/api/chat/handover");
        AllowAnonymous(); // Update as per authentication requirements
        Options(x => x.WithTags("Chat"));
    }

    public override async Task HandleAsync(HandoverRequest req, CancellationToken ct)
    {
        var conversation = await _dbContext.Conversations
            .Include(c => c.ConversationParticipants)
                .ThenInclude(cp => cp.Participant)
            .FirstOrDefaultAsync(c => c.Id == req.ConversationId, ct);

        if (conversation == null)
        {
            ThrowError("Conversation not found.", statusCode: 404);
        }

        if (!conversation.Status.Equals("active", StringComparison.OrdinalIgnoreCase))
        {
            ThrowError($"Cannot handover a conversation with status '{conversation.Status}'.", statusCode: 400);
        }

        if (conversation.CurrentRepresentativeId.HasValue)
        {
            var response = new HandoverResponse
            {
                RepresentativeId = conversation.CurrentRepresentativeId.Value
            };
            await SendAsync(response, cancellation: ct);
            return;
        }

        var availableRepresentative = await _dbContext.Participants
            .Where(p => p.Role.Equals("representative", StringComparison.OrdinalIgnoreCase))
            .Where(p => !_dbContext.ConversationParticipants
                .Any(cp => cp.ParticipantId == p.Id && cp.Status.Equals("busy", StringComparison.OrdinalIgnoreCase)))
            .FirstOrDefaultAsync(ct);

        if (availableRepresentative == null)
        {
            ThrowError("No representatives available at the moment.", statusCode: 503);
        }

        var conversationParticipant = new ConversationParticipant
        {
            ConversationId = conversation.Id,
            ParticipantId = availableRepresentative.Id,
            JoinedAt = DateTime.UtcNow,
            Status = "busy"
        };
        _dbContext.ConversationParticipants.Add(conversationParticipant);

        conversation.CurrentRepresentativeId = availableRepresentative.Id;
        _dbContext.Conversations.Update(conversation);

        var handoverMessage = new Message
        {
            ConversationId = conversation.Id,
            ParticipantId = availableRepresentative.Id,
            MessageText = "You have been connected to a human representative.",
            Timestamp = DateTime.UtcNow
        };
        _dbContext.Messages.Add(handoverMessage);

        await _dbContext.SaveChangesAsync(ct);

        var handoverResponse = new HandoverResponse
        {
            RepresentativeId = availableRepresentative.Id
        };

        await SendAsync(handoverResponse, cancellation: ct);
    }
}