using Grpc.Core;

namespace GrpcChatServer;

internal class GrpcChatServerImpl : ChatGrpc.ChatGrpcBase
{
    private const int Port = 12345;
    
    public static void Main(string[] args)
    {
        var serverService = new ServerService();
        var controller = new ServerController(serverService);
        var service = ChatGrpc.BindService(controller);
            
        var serverPort = new ServerPort("127.0.0.1", Port, ServerCredentials.Insecure);
        
        var server = new Server
        {
            Services = { service },
            Ports = { serverPort }
        };
        
        server.Start();

        Console.WriteLine("Greeter server listening on port " + Port);
        Console.WriteLine("Press any key to stop the server...");
        Console.ReadKey();

        server.ShutdownAsync().Wait();
    }
}