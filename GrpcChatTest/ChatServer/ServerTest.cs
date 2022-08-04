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
    
    [Test]
    public Task TestEnroll()
    {
        _client.Enroll(new Empty());
        
        return Task.CompletedTask;
    }

    [Test]
    public async Task TestChangeNick()
    {
        _client.Enroll(new Empty());

        var response = await _client.ChangeNickAsync(new ChangeNickRequest()
        {
            NewNickname = "bar"
        });

        if (response.Failed is not null)
        {
            Assert.Fail(response.Failed.Reason);
        }
        else
        {
            Assert.Pass();
        }
    }
    
    [Test]
    public async Task TestShowRooms()
    {
        var response = await _client.ShowRoomsAsync(new Empty());
        if (response.RoomInfos is null)
        {
            Assert.Fail();
        }
        Assert.Pass();
    }

    [Test]
    public async Task TestCreateRoom()
    {
        var roomCount = _client.ShowRooms(new Empty()).RoomInfos.Count;
        await _client.CreateRoomAsync(new CreateRoomRequest
        {
            Name = "testRoom"
        });
        var changedCount = _client.ShowRooms(new Empty()).RoomInfos.Count;
        
        Assert.AreEqual(roomCount + 1, changedCount);
        
    }
}