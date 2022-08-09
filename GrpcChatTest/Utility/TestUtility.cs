using Grpc.Core;
using GrpcChatServer;

namespace GrpcChatTest.Utility;

public static class TestUtility
{
    public static ServerController CreateServerController(ServerService? service = null)
    {
        service ??= new ServerService();
        return new ServerController(service);
    }
    
    public static Server CreateServer(int port, ServerController? controller = null)
    {
        controller ??= CreateServerController();
        
        return new Server
        {
            Services = { ChatGrpc.BindService(controller) },
            Ports = { new ServerPort("127.0.0.1", port, ServerCredentials.Insecure) }
        };
    }
    
    public static (ChatGrpc.ChatGrpcClient, Channel) CreateClient(int port, string name = "default")
    {
        // TODO : 키값 나중에 가능하면 지정된 키로 바꿔야함 
        var options = new ChannelOption[] { new("__querypie__", name) };
        var channel = new Channel($"127.0.0.1:{port}", ChannelCredentials.Insecure, options);
        // var channel = new Channel($"127.0.0.1:{port}", ChannelCredentials.Insecure);
        var client = new ChatGrpc.ChatGrpcClient(channel);

        return (client, channel);
    }
    
    public static Dictionary<ChatGrpc.ChatGrpcClient, Channel> MakeManyClient(int clientCount)
    {
        Dictionary<ChatGrpc.ChatGrpcClient, Channel> clientChannels = new();
        
        for (var i = 0; i < clientCount; i++)
        {
            var (client, channel) = CreateClient(12345, $"client{i+1}");
            clientChannels.TryAdd(client, channel);
        }

        return clientChannels;
    }
}