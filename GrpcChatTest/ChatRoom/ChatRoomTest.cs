using System.Collections.Concurrent;
using GrpcChatServer;
using NUnit.Framework;

namespace GrpcChatTest.ChatRoom;

public class ChatRoomTest
{
    private static void TestAction(MessageContext context) => Console.WriteLine(context.Message);

    private GrpcChatServer.ChatRoom _room = null!;
    private readonly ConcurrentDictionary<string, GrpcChatServer.ChatRoom> _testRoomList = new();
    private readonly ConcurrentDictionary<string, Client> _testClientList = new();

    private Client _testClient1 = null!;
    private Client _testClient2 = null!;

    [SetUp]
    public void SetUp()
    {
        _room = new GrpcChatServer.ChatRoom("TEST");

        _testClient1 = new Client("1234", "danny");
        _testClient2 = new Client("12345", "kimDanny");

        _testRoomList.TryAdd(_room.Name, _room);
        _testClientList.TryAdd(_testClient1.Peer, _testClient1);
        _testClientList.TryAdd(_testClient2.Peer, _testClient2);
    }

    [Test]
    public void TestEnter()
    {
        var expected = _room.ParticipantsCount() + 1;
        var testClient = new Client("1234", "danny");

        _room.Enter(testClient, TestAction);

        Assert.AreEqual(expected, _room.ParticipantsCount());
    }

    [Test]
    public void TestExitNotZeroClient()
    {
        var testClient1 = new Client("1234", "danny");
        var testClient2 = new Client("12345", "kimDanny");
        
        _room.Enter(testClient1, TestAction);
        _room.Enter(testClient2, TestAction);

        var expected = _room.ParticipantsCount() - 1;

        _room.Exit(testClient1.Peer);

        Assert.AreEqual(expected, _room.ParticipantsCount());
    }

    [Test]
    public async Task TestExitZeroClient()
    {
        _room.Handler.OnRemove += () => TestRemove(_room.Name);

        _room.Enter(_testClient1, TestAction);

        _testRoomList.TryAdd(_room.Name, _room);

        var expected = _testRoomList.Count - 1;

        _room.Exit(_testClient1.Peer);

        await Task.Delay(6000);

        Assert.AreEqual(expected, _testRoomList.Count);
    }

    private void TestRemove(string name)
    {
        _testRoomList.TryRemove(name, out _);
    }

    [TestCase(5, ExpectedResult = 5)]
    public int TestBroadcast(int count)
    {
        var messagesFromClient1 = new List<string>();

        _room.Enter(_testClient1, TestAction);
        _room.Enter(_testClient2, TestAction);

        _testClient1.OnSend += OnSendHandler;

        for (var i = 0; i < count; i++)
        {
            _room.Broadcast(_testClient1.Peer, "testMessage");
        }

        _testClient1.OnSend -= OnSendHandler;

        return messagesFromClient1.Count;

        void OnSendHandler(MessageContext context) => messagesFromClient1.Add(context.Message);
    }

    [TestCase(3,ExpectedResult = 6)]
    [TestCase(5,ExpectedResult = 10)]
    public int TestPrevChat(int count)
    {
        var prevChatQueue = new Queue<string>();
        
        _room.Enter(_testClient1, TestAction);
        _room.Enter(_testClient2, TestAction);

        _testClient1.OnSend += PrevChatHandler;
        _testClient2.OnSend += PrevChatHandler;

        for (var i = 0; i < count; i++)
        {
            _room.Broadcast(_testClient1.Peer, "testMessage1");
            _room.Broadcast(_testClient2.Peer, "testMessage2");
        }
        
        _testClient1.OnSend -= PrevChatHandler;
        _testClient2.OnSend -= PrevChatHandler;

        void PrevChatHandler(MessageContext context) => prevChatQueue.Enqueue(context.Message);

        return prevChatQueue.Count;
    }
}