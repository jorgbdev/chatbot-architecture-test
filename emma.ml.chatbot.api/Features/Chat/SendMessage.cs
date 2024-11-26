// src/app/features/chat/send-message-endpoint.cs
using emma.ml.chatbot.api.Database;
using emma.ml.chatbot.api.Entities.Chat;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace emma.ml.chatbot.api.Features.Chat;

public class SendMessageRequest
{
    public required Guid ConversationId { get; set; }
    public required string UserId { get; set; }
    public required string MessageText { get; set; }
}

public class SendMessageResponse
{
    public int MessageId { get; set; }
}

public class SendMessageValidator : Validator<SendMessageRequest>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("ConversationId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.MessageText)
            .NotEmpty()
            .WithMessage("MessageText cannot be empty.");
    }
}

public class SendMessageEndpoint : Endpoint<SendMessageRequest, SendMessageResponse>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHubContext<ChatHub> _hubContext;

    public SendMessageEndpoint(ApplicationDbContext dbContext, IHubContext<ChatHub> hubContext)
    {
        _dbContext = dbContext;
        _hubContext = hubContext;
    }

    public override void Configure()
    {
        Post("/api/chat/send");
        AllowAnonymous(); // Adjust as per your authentication requirements
        Options(x => x.WithTags("Chat"));
    }

    public override async Task HandleAsync(SendMessageRequest req, CancellationToken ct)
    {
        // Retrieve the conversation with participants
        var conversation = await _dbContext.Conversations
            .Include(c => c.ConversationParticipants)
                .ThenInclude(cp => cp.Participant)
            .FirstOrDefaultAsync(c => c.Id == req.ConversationId, ct);

        if (conversation == null)
        {
            ThrowError("Conversation not found.", statusCode: 404);
        }

        if (conversation.Status.Equals("ended", StringComparison.OrdinalIgnoreCase))
        {
            ThrowError("Conversation has ended.", statusCode: 400);
        }

        // Check if the user is part of the conversation
        var conversationParticipant = conversation.ConversationParticipants
            .FirstOrDefault(cp => cp.Participant!.UserId == req.UserId && cp.LeftAt == null);

        if (conversationParticipant == null)
        {
            ThrowError("User is not part of the conversation.", statusCode: 403);
        }

        // Create the message
        var message = new Message
        {
            ConversationId = conversation.Id,
            ParticipantId = conversationParticipant.ParticipantId,
            MessageText = req.MessageText,
            Status = "sent",
            Timestamp = DateTime.UtcNow
        };

        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync(ct);

        // Broadcast the message via SignalR
        await _hubContext.Clients.Group(conversation.Id.ToString())
            .SendAsync("ReceiveMessage", req.UserId, req.MessageText, message.Status, message.Timestamp, ct);

        // Send the success response
        await SendAsync(new SendMessageResponse
        {
            MessageId = message.Id
        }, cancellation: ct);
    }
}