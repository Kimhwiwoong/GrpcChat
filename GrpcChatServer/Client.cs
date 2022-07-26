namespace GrpcChatServer;

public class Client
{
    public Client(string peer, string name)
    {
        Peer = peer;
        Name = name;
    }

    public void Send(string chatMsg)
    {
        OnSend?.Invoke(chatMsg);
    }

    public string Peer { get; }
    public string Name { get; set; }
    public event Action<string>? OnSend;
}