namespace GrpcChatServer;

public class Client
{
    public event Action<MessageContext>? OnSend;
    
    public string Peer { get; }
    public string Name { get; set; }
    
    public Client(string peer, string name)
    {
        Peer = peer;
        Name = name;
    }

    public void Send(MessageContext context)
    {
        OnSend?.Invoke(context);
    }
}