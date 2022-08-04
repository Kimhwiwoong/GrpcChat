using Grpc.Core;

namespace GrpcChat
{
    class GrpcChat
    {
        public static void Main()
        {
            var channel = new Channel("127.0.0.1:12345", ChannelCredentials.Insecure);
            var client = new ChatGrpc.ChatGrpcClient(channel);

            var service = new ClientService(client);
            var controller = new ClientController(service);

            controller.Start();
        }
    }
}

