using emma.ml.chatbot.api.Database;
using emma.ml.chatbot.api.Entities.Chat;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace emma.ml.chatbot.api.Features.Chat;

public class StartConversationRequest
{
    public required string UserId { get; set; }
}

public class StartConversationResponse
{
    public Guid ConversationId { get; set; }
}

public class StartConversationEndpoint : Endpoint<StartConversationRequest, StartConversationResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public StartConversationEndpoint(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("/api/chat/conversation/start");
        AllowAnonymous();
        Options(x => x.WithTags("Chat"));
    }

    public override async Task HandleAsync(StartConversationRequest req, CancellationToken ct)
    {
        var conversationParticipant = await _dbContext.ConversationParticipants
            .Include(cp => cp.Conversation)
            .Include(cp => cp.Participant)
            .FirstOrDefaultAsync(cp => cp.Participant!.UserId == req.UserId
                                        && cp.LeftAt == null
                                        && cp.Conversation!.Status != "ended", ct);

        if (conversationParticipant != null)
        {
            await SendAsync(new StartConversationResponse
            {
                ConversationId = conversationParticipant.ConversationId
            }, cancellation: ct);
            return;
        }

        var participant = await _dbContext.Participants
            .FirstOrDefaultAsync(p => p.UserId == req.UserId, ct);

        if (participant == null)
        {
            participant = new Participant
            {
                UserId = req.UserId,
                Role = "client"
            };
            _dbContext.Participants.Add(participant);
            await _dbContext.SaveChangesAsync(ct);
        }

        var conversation = new Conversation
        {
            Status = "awaiting_representative",
            StartedAt = DateTime.UtcNow
        };
        _dbContext.Conversations.Add(conversation);
        await _dbContext.SaveChangesAsync(ct);

        var conversationParticipantNew = new ConversationParticipant
        {
            ConversationId = conversation.Id,
            ParticipantId = participant.Id,
            JoinedAt = DateTime.UtcNow,
            Status = "active"
        };
        _dbContext.ConversationParticipants.Add(conversationParticipantNew);
        await _dbContext.SaveChangesAsync(ct);

        await SendAsync(new StartConversationResponse
        {
            ConversationId = conversation.Id
        }, cancellation: ct);
    }
}