using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcChatServer;
using NUnit.Framework;

namespace GrpcChatTest.ChatServer;

public class ServerTest
{
    private Channel _channel = null!;
    
    private Server _server = null!;
    private ServerController _controller = null!;
    
    private ChatGrpc.ChatGrpcClient _client = null!;

    [SetUp]
    public void Setup()
    {
        var service = new ServerService();
        _controller = new ServerController(service);
        
        _server = new Server
        {
            Services = { ChatGrpc.BindService(_controller) },
            Ports = { new ServerPort("127.0.0.1", 12345, ServerCredentials.Insecure) }
        };
        
        _server.Start();
        
        _channel = new Channel("127.0.0.1:12345", ChannelCredentials.Insecure);
        _client = new ChatGrpc.ChatGrpcClient(_channel);
    }

    [TearDown]
    public void Teardown()
    {
        _channel.ShutdownAsync().Wait();
        _server.ShutdownAsync().Wait();
    }
    
    public Task TestEnroll()
    {
        _client.Enroll(new Empty());

        return Task.CompletedTask;
    }

    public async Task TestChangeNick()
    {
        var response = await _client.ChangeNickAsync(new ChangeNickRequest()
        {
            OldName = "foo",
            NewName = "bar",
        });

        if (response.Failed is { } failed)
        {
            Assert.Fail(failed.Reason);
            return;
        }

        // if (response.Success is {} success)
        // {
        //     Assert.Equals("foo", success.OldName);
        //     Assert.Equals("bar", success.NewName);
        //     return;
        // }

        throw new InvalidOperationException();
    }
}