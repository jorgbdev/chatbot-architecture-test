//using emma.ml.chatbot.api.Database;
//using FastEndpoints;
//using Microsoft.EntityFrameworkCore;

//namespace emma.ml.chatbot.api.Features.Chat;

//public class LeaveConversationRequest
//{
//    public required Guid ConversationId { get; set; }
//    public required string UserId { get; set; }
//}

//public class LeaveConversationResponse
//{
//    public string Message { get; set; } = "User left the conversation.";
//}

//public class LeaveConversationEndpoint : Endpoint<LeaveConversationRequest, LeaveConversationResponse>
//{
//    private readonly ApplicationDbContext _dbContext;

//    public LeaveConversationEndpoint(ApplicationDbContext dbContext)
//    {
//        _dbContext = dbContext;
//    }

//    public override void Configure()
//    {
//        Post("/api/chat/conversation/leave");
//        AllowAnonymous();
//        Options(x => x.WithTags("Chat"));
//    }

//    public override async Task HandleAsync(LeaveConversationRequest req, CancellationToken ct)
//    {
//        var participant = await _dbContext.Participants
//            .FirstOrDefaultAsync(p => p.UserId == req.UserId && p.ConversationId == req.ConversationId && p.LeftAt == null, ct);

//        if (participant == null)
//        {
//            await SendAsync(new LeaveConversationResponse { Message = "User not found in the conversation." }, statusCode: 404, cancellation: ct);
//            return;
//        }

//        participant.LeftAt = DateTime.UtcNow;
//        _dbContext.Participants.Update(participant);
//        await _dbContext.SaveChangesAsync(ct);

//        await SendAsync(new LeaveConversationResponse(), cancellation: ct);
//    }
//}
