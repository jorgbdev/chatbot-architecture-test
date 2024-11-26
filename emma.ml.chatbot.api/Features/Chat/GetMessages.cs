using emma.ml.chatbot.api.Database;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace emma.ml.chatbot.api.Features.Chat;

public class GetMessagesRequest
{
    public required Guid ConversationId { get; set; }
}

public class GetMessagesResponse
{
    public List<MessageDto> Messages { get; set; } = new List<MessageDto>();
}

public class MessageDto
{
    public int Id { get; set; }
    public required string ParticipantId { get; set; }
    public required string MessageText { get; set; }
    public required string Status { get; set; }
    public DateTime Timestamp { get; set; }
}

public class GetMessagesValidator : Validator<GetMessagesRequest>
{
    public GetMessagesValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("ConversationId is required.");
    }
}

public class GetMessagesEndpoint : Endpoint<GetMessagesRequest, GetMessagesResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetMessagesEndpoint(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("/api/chat/messages");
        AllowAnonymous();
        Options(x => x.WithTags("Chat"));
    }

    public override async Task HandleAsync(GetMessagesRequest req, CancellationToken ct)
    {
        var messages = await _dbContext.Messages
            .Include(m => m.Participant)
            .Where(m => m.ConversationId == req.ConversationId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync(ct);

        var response = new GetMessagesResponse
        {
            Messages = messages.Select(m => new MessageDto
            {
                Id = m.Id,
                ParticipantId = m.Participant!.UserId,
                MessageText = m.MessageText,
                Status = m.Status,
                Timestamp = m.Timestamp
            }).ToList()
        };

        await SendAsync(response, cancellation: ct);
    }
}