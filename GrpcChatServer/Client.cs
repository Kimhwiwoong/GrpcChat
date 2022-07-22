using Grpc.Core;

namespace GrpcChatServer;

public class Client
{
    public Client(string peer, string name)
    {
        Peer = peer;
        Name = name;
        JoinedRoom = "";
    }

    public string Peer { get; }
    public string JoinedRoom { get; set; }
    public string Name { get; set; }
    //public IServerStreamWriter<ChatMessage>? ClientStreamWriter { get; set; }
}