using System.Collections.Concurrent;
using GrpcChatServer;
using NUnit.Framework;

namespace GrpcChatTest.ChatRoom;

public class ChatRoomTest
{
    private static void TestAction(MessageData data) => Console.WriteLine(data.Message);

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

    [TearDown]
    public void TearDown()
    {
        _testRoomList.Clear();
        _testClientList.Clear();
    }

    // TODO: Idea - 쓰레드 safe한지 테스트해보는건 어떨까? -> Thread safe test
    // TODO: Idea 2 - 한 클라이언트가 두번 Enter하면? -> Edge case? Error handling test?

    [Test]
    public void TestEnter()
    {
        var expected = _room.ParticipantsCount() + 1;
        var testClient = new Client("1234", "danny");

        _room.Enter(testClient, TestAction);

        Assert.AreEqual(expected, _room.ParticipantsCount());
    }

    [Test]
    public async Task TestAsyncEnter()
    {
        var tasks = new Task[10];
        var tasks1 = new Task[10];
        
        for (var i = 0; i < 10; ++i)
        {
            tasks[i] = EnterMany();
            tasks[i].Start();
            
            tasks1[i] = EnterMany1();
            tasks1[i].Start();
        }

        await Task.WhenAll(tasks);
        await Task.WhenAll(tasks1);
        
        Assert.AreEqual(20,_room.ParticipantsCount());
    }

    private Task EnterMany()
    {
        return new Task(() =>
        {
            for (var i = 0; i < 10; ++i)
            {
                Thread.Sleep(50);
                _room.Enter(new Client(i.ToString(), i.ToString()), TestAction);
            }
        });
    }
    
    private Task EnterMany1()
    {
        return new Task(() =>
        {
            for (var i = 100; i > 90; i--)
            {
                Thread.Sleep(50);
                _room.Enter(new Client(i.ToString(), i.ToString()), TestAction);
            }
        });
    }

    [Test]
    public void TestExitNotZeroClient()
    {
        _room.Enter(_testClient1, TestAction);
        _room.Enter(_testClient2, TestAction);

        var expected = _room.ParticipantsCount() - 1;

        _room.Exit(_testClient1.Peer);

        Assert.AreEqual(expected, _room.ParticipantsCount());
    }

    [Test]
    public async Task TestExitZeroClient()
    {
        var tcs = new TaskCompletionSource();
        
        _room.Handler.OnRemove += () =>
        {
            TestRemove(_room.Name);
            tcs.SetResult();
        };

        _room.Enter(_testClient1, TestAction);

        _testRoomList.TryAdd(_room.Name, _room);

        var expected = _testRoomList.Count - 1;

        _room.Exit(_testClient1.Peer);

        await tcs.Task;

        Assert.AreEqual(expected, _testRoomList.Count);
    }

    private void TestRemove(string name)
    {
        _testRoomList.TryRemove(name, out _);
    }

    [TestCase(5)]
    public void TestBroadcast(int count)
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
        
        
        Assert.AreEqual(count, messagesFromClient1.Count);

        void OnSendHandler(MessageData context) => messagesFromClient1.Add(context.Message);
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

        void PrevChatHandler(MessageData context) => prevChatQueue.Enqueue(context.Message);

        return prevChatQueue.Count;
    }
    
    [Test]
    public async Task TestActionCalled()
    {
        _testRoomList.Clear();
        var tcs = new TaskCompletionSource();
        var newRoom = new GrpcChatServer.ChatRoom("newRoom");
        
        newRoom.Handler.OnRemove += () =>
        {
            _testRoomList.TryRemove("newRoom", out _);
            Console.WriteLine("OnRemove called");
            tcs.SetResult();
        };
        
        _testRoomList.TryAdd("newRoom", newRoom);

        newRoom.Enter(_testClient1, TestAction);
        var afterEnter = _testRoomList.Count;

        newRoom.Exit(_testClient1.Peer);
        await tcs.Task;
        var afterExit = _testRoomList.Count;
        
        Assert.AreNotEqual(afterExit, afterEnter);
    }
}