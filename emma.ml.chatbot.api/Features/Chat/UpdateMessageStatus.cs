using emma.ml.chatbot.api.Database;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace emma.ml.chatbot.api.Features.Chat;

public class UpdateMessageStatusRequest
{
    public required int MessageId { get; set; }
    public required string Status { get; set; }
}

public class UpdateMessageStatusResponse
{
    public int MessageId { get; set; }
    public required string Status { get; set; }
}

public class UpdateMessageStatusValidator : Validator<UpdateMessageStatusRequest>
{
    public UpdateMessageStatusValidator()
    {
        RuleFor(x => x.MessageId)
            .GreaterThan(0)
            .WithMessage("Valid MessageId is required.");

        RuleFor(x => x.Status)
            .Must(status => new[] { "delivered", "read" }.Contains(status))
            .WithMessage("Invalid status specified.");
    }
}

public class UpdateMessageStatusEndpoint : Endpoint<UpdateMessageStatusRequest, UpdateMessageStatusResponse>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHubContext<ChatHub> _hubContext;

    public UpdateMessageStatusEndpoint(ApplicationDbContext dbContext, IHubContext<ChatHub> hubContext)
    {
        _dbContext = dbContext;
        _hubContext = hubContext;
    }

    public override void Configure()
    {
        Put("/api/chat/message/status");
        AllowAnonymous();
        Options(x => x.WithTags("Chat"));
    }

    public override async Task HandleAsync(UpdateMessageStatusRequest req, CancellationToken ct)
    {
        var message = await _dbContext.Messages
            .Include(m => m.Conversation)
            .FirstOrDefaultAsync(m => m.Id == req.MessageId, ct);

        if (message == null)
        {
            ThrowError("Message not found.", statusCode: 404);
        }

        message.Status = req.Status;
        _dbContext.Messages.Update(message);
        await _dbContext.SaveChangesAsync(ct);

        await _hubContext.Clients.Group(message.ConversationId.ToString())
            .SendAsync("UpdateMessageStatus", message.Id, message.Status, ct);

        var response = new UpdateMessageStatusResponse
        {
            MessageId = message.Id,
            Status = message.Status
        };

        await SendAsync(response, cancellation: ct);
    }
}