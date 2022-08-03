namespace GrpcChatServer;

public class Client
{
    public Client(string peer, string name)
    {
        Peer = peer;
        Name = name;
    }

    public void Send(MessageContext context)
    {
        OnSend?.Invoke(context);
    }

    public string Peer { get; }
    public string Name { get; set; }
    public event Action<MessageContext>? OnSend;
}