using Grpc.Core;
using GrpcChat;
using GrpcChat.Exceptions;
using GrpcChatTest.Utility;
using NUnit.Framework;

namespace GrpcChatTest.ChatClient;

public class ClientServiceTest
{
    private ClientService _clientService = null!;
    private Channel _channel = null!;
    private Server _server = null!;
    private ChatGrpc.ChatGrpcClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _server = TestUtility.CreateServer(12345);
        _server.Start();
        (_client, _channel) = TestUtility.CreateClient(12345);
        _clientService = new ClientService(_client);
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
        try
        {
            _clientService.Enroll();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        Assert.AreNotEqual(null, _clientService.GetCurrentNickname());
    }

    [Test]
    public void TestCreateRoom()
    {
        const string test = "room";
        try
        {
            _clientService.CreateRoom(test);
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    // [TestCase("same", "same")]
    [TestCase("same", "different")]
    public void TestCreateRoomTwice(string a, string b)
    {
        try
        {
            _clientService.CreateRoom(a);
            _clientService.CreateRoom(b);
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Test]
    public void TestChangeNick()
    {
        string after;
        const string newNickname = "newNickname";
        
        _clientService.Enroll();
        
        try
        {
            _clientService.ChangeNickname(newNickname);
            
            after = _clientService.GetCurrentNickname();
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine(e);
            throw;
        }

        if(after != newNickname) Assert.Fail();
        Assert.Pass();
    }
    
    [Test]
    public void TestShowRooms()
    {
        const int count = 5;
        
        for (var i = 0; i < count; i++)
        {
            _clientService.CreateRoom($"test{i}");
        }

        var rooms = _clientService.ShowRooms().Count();
        Assert.AreEqual(count, rooms);
    }
    
    [Test]
    public void TestEnterRoom()
    {
        const string roomName = "room";
        ChatClientTestUtility.EnrollAndCreateRoom(roomName, _clientService);

        try
        {
            var room = _clientService.EnterRoom(roomName);
            room.Exit();
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
        
        Assert.Pass();
    }
    
    [Test]
    public void TestEnterRoomInvalid()
    {
        const string roomName = "room";
        _clientService.Enroll();

        Assert.Catch<EnterRoomException>(() =>
        {
            var room = _clientService.EnterRoom(roomName);
            room.Exit();
        });
    }

    [Test]
    public void TestSendChat()
    {
        const string roomName = "room";
        ChatClientTestUtility.EnrollAndCreateRoom(roomName, _clientService);
        
        try
        {
            var room = _clientService.EnterRoom(roomName);
            room.SendMessage("test");
            room.Exit();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Assert.Fail();
        }
        Assert.Pass();
    }
    
    // 여기는 뭐 할수있는게 없다 안타깝네
    public void TestReceiveChat()
    {
        const string roomName = "room";
        ChatClientTestUtility.EnrollAndCreateRoom(roomName, _clientService);

        try
        {
           _clientService.EnterRoom(roomName);
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}