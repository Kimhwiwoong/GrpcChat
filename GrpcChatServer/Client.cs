namespace GrpcChatServer;

public class Client
{
    public event Action<MessageData>? OnSend;
    
    public string Peer { get; }
    public string Name { get; set; }

    public Client(string peer, string name)
    {
        Peer = peer;
        Name = name;
    }

    public void Send(MessageData data)
    {
        OnSend?.Invoke(data);
    }
}