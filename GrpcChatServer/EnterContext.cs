namespace GrpcChatServer;

public class EnterContext : IDisposable
{
    private Client CurrentClient { get; }

    private ChatRoom CurrentRoom { get; }
    
    private readonly Action<MessageContext> _action;

    public EnterContext(Client currentClient, ChatRoom currentRoom, Action<MessageContext> action)
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
        GC.SuppressFinalize(this);
        
        CurrentRoom.Exit(CurrentClient.Peer);
        CurrentClient.OnSend -= _action;
    }

    public void Initialize()
    {
        var clientEnterMessage = $"{CurrentClient.Name} Entered ChatRoom";
        
        Broadcast(clientEnterMessage);
        
        // Load previous 10(able to set in ChatRoom class) chats.
        CurrentRoom.SendPrevChat(CurrentClient.Peer);
    }
}