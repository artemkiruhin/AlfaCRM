﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace AlfaCRM.Api.Hubs;

public class ChatHub : Hub
{
    public async Task JoinChat(string username, string chatName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatName);
    }

    public async Task SendMessage(string senderId, string message, string chatName, string senderUsername)
    {
        var createdAt = DateTime.UtcNow;

        await Clients.Group(chatName).SendAsync("ReceiveMessage", 
            senderId, 
            message, 
            createdAt.ToString("o"),
            senderUsername);
    }

    public async Task LeaveChat(string chatName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatName);
    }
    
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}