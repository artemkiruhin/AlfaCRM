namespace AlfaCRM.Api.Hubs;

public interface IChatClient
{
    Task ReceiveMessage(string username, string message);
    Task UserJoined(string username);
    Task UserLeft(string username);
}