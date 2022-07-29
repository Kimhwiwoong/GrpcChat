using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcChatServer;
using NUnit.Framework;

namespace GrpcChatTest.ChatClient;

public class ClientTest
{
    private ChatGrpc.ChatGrpcClient _client = null!;
    private Channel _channel = null!;
    private Server _server = null!;


    [SetUp]
    public void Setup()
    {
        _server = TestUtility.CreateServer(12345);
        _server.Start();

        (_channel, _client) = TestUtility.CreateClient(12345);
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
        
        var showResult = await _client.ShowRoomsAsync(new Empty());
        if (showResult.RoomInfos.All(info => info.Name != testRoomName))
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
    
    [Test]
    public async Task TestShowRooms()
    {
        // 개수 확인하는 방법보다 이름을 아니까 이름들어있는지 확인하는게 좋아보이는데
        const int createCount = 5;
        
        var request = new Empty();
        var response = await _client.ShowRoomsAsync(request);
        
        for (var i = 0; i < createCount; i++)
        {
            await _client.CreateRoomAsync(new CreateRoomRequest
            {
                Name = i.ToString()
            });
        }
        
        var secondResponse = await _client.ShowRoomsAsync(request);
        
        Assert.AreEqual(response.RoomInfos.Count + createCount, secondResponse.RoomInfos.Count);
    }

    
    
}