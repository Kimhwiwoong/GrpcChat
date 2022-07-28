using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcChatServer;
using NUnit.Framework;

namespace GrpcChatTest.ChatClient;

public class ClientTest
{
    private ChatGrpc.ChatGrpcClient _client;
    private Channel _channel = null!;
    private Server _server = null!;
    private ServerController _controller = null!;


    [SetUp]
    public void Setup()
    {
        _controller = new ServerController(new ServerService());

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
    public void TearDown()
    {
        _channel.ShutdownAsync().Wait();
        _server.ShutdownAsync().Wait();
    }

    [Test]
    public void TestEnroll()
    {
        var request = new Empty();
        var response = _client.Enroll(request);
        
        if (response.Nickname is null)
        {
            Assert.Fail();
            return;
        }
        Assert.Pass();
    }
    
    [Test]
    public async Task TestCreateRoomAdded()
    {
        const string testRoomName = "testCreate";
        var request = new CreateRoomRequest
        {
            Name = testRoomName
        };

        await _client.CreateRoomAsync(request);
        
        var showResult = _client.ShowRooms(new Empty());
        if (showResult.Rooms.All(info => info.Name != testRoomName))
        {
            Console.WriteLine("hi");
            Assert.Fail();
            return;
        }

        Assert.Pass();
    }

    [Test]
    public async Task TestCreateRoomReceiveSuccess()
    {
        var request = new CreateRoomRequest
        {
            Name = "testsuite"
        };

        var result = await _client.CreateRoomAsync(request);
        
        if (result.ResponseCase is SuccessFailResponse.ResponseOneofCase.Failed)
        {
            Assert.Fail();
        }
        
        Assert.Pass();
    }
}