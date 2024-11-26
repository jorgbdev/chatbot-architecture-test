using Microsoft.AspNetCore.SignalR;

namespace emma.ml.chatbot.api.Features.Chat;
public class ChatHub : Hub
{
    //public async Task SendMessage(string conversationId, string userId, string message)
    //{
    //    await Clients.Group(conversationId).SendAsync("ReceiveMessage", userId, message, "sent", DateTime.UtcNow.ToString("o"));
    //}

    public async Task Typing(string conversationId, string userId)
    {
        await Clients.Group(conversationId).SendAsync("UserTyping", userId);
    }

    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        await Clients.Group(conversationId).SendAsync("UserJoined", Context.ConnectionId);
    }

    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
        await Clients.Group(conversationId).SendAsync("UserLeft", Context.ConnectionId);
    }
}