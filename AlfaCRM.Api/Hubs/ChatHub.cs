using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace AlfaCRM.Api.Hubs;

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private static readonly ConcurrentDictionary<string, string> UserConnections = new();
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var username = Context.User?.Identity?.Name ?? "anonymous";
        UserConnections.TryAdd(Context.ConnectionId, username);
        _logger.LogInformation("User {Username} connected with ID {ConnectionId}", username, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        UserConnections.TryRemove(Context.ConnectionId, out _);
        
        if (exception != null)
        {
            _logger.LogError(exception, "User disconnected with error");
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinChat(string chatName)
    {
        if (string.IsNullOrWhiteSpace(chatName))
            throw new HubException("Chat name is required");

        var username = Context.User?.Identity?.Name ?? "anonymous";
        
        await Groups.AddToGroupAsync(Context.ConnectionId, chatName);
        await Clients.Group(chatName).UserJoined(username);
        
        _logger.LogInformation("User {Username} joined chat {ChatName}", username, chatName);
    }

    public async Task LeaveChat(string chatName)
    {
        if (string.IsNullOrWhiteSpace(chatName))
            throw new HubException("Chat name is required");

        var username = Context.User?.Identity?.Name ?? "anonymous";
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatName);
        await Clients.Group(chatName).UserLeft(username);
        
        _logger.LogInformation("User {Username} left chat {ChatName}", username, chatName);
    }

    public async Task SendMessage(string message, string chatName)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new HubException("Message cannot be empty");
        
        if (string.IsNullOrWhiteSpace(chatName))
            throw new HubException("Chat name is required");

        var username = Context.User?.Identity?.Name ?? "anonymous";
        
        await Clients.Group(chatName).ReceiveMessage(username, message);
        
        _logger.LogInformation("Message sent to chat {ChatName} by {Username}", chatName, username);
    }
}