using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcChatTest.Utility;
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
        (_client, _channel) = TestUtility.CreateClient(12345);
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

    
    public async Task TestEnterRoom()
    {
        const string roomName = "room";
        try
        {
            ChatClientTestUtility.Enroll(_client);
            ChatClientTestUtility.CreateRoom(_client, roomName);
            
            await ChatClientTestUtility.Enter(_client, "aa");
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
        
        Assert.Pass();
    }
}