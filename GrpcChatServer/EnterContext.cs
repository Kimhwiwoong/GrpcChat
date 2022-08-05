namespace GrpcChatServer;

public sealed class EnterContext : IDisposable
{
    private Client CurrentClient { get; }

    private ChatRoom CurrentRoom { get; }
    
    private readonly Action<MessageData> _action;

    public EnterContext(Client currentClient, ChatRoom currentRoom, Action<MessageData> action)
    {
        _action = action;
        CurrentClient = currentClient;
        CurrentRoom = currentRoom;

        CurrentClient.OnSend += action;
    }

    public void Broadcast(string message)
    {
        CurrentRoom.Broadcast(CurrentClient.Peer, message);
    }
    
    public void Dispose()
    {
        CurrentRoom.Exit(CurrentClient.Peer);
        CurrentClient.OnSend -= _action;
    }

    public void Initialize()
    {
        var clientEnterMessage = $"{CurrentClient.Name} Entered ChatRoom";

        // Load previous 10(able to set in ChatRoom class) chats.
        CurrentRoom.SendPrevChat(CurrentClient.Peer);
        
        Broadcast(clientEnterMessage);
    }
}