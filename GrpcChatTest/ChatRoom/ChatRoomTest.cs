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
        const int taskAmount = 20;
        const int iteration = 1000;
        
        var tasks = new Task[taskAmount];

        for (var i = 0; i < taskAmount; ++i)
        {
            tasks[i] = EnterMany(i * 10000, iteration);
            tasks[i].Start();
        }

        await Task.WhenAll(tasks);

        Assert.AreEqual(taskAmount * iteration,_room.ParticipantsCount());
    }

    private Task EnterMany(int offset, int amount)
    {
        return new Task(() =>
        {
            for (var i = 0; i < amount; ++i)
            {
                var id = (i + offset).ToString();
                
                _room.Enter(new Client(id, id), TestAction);
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

        _room.Enter(_testClient1, OnSendHandler);
        _room.Enter(_testClient2, OnSendHandler);

        for (var i = 0; i < count; i++)
        {
            _room.Broadcast(_testClient1.Peer, "testMessage");
        }

        Assert.AreEqual(count, messagesFromClient1.Count);

        void OnSendHandler(MessageData context)
        {
            Console.WriteLine("Invoked!");
            messagesFromClient1.Add(context.Message);
        }
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

    private void AddFunctionHandler(TaskCompletionSource tcs)
    {
        _testRoomList.TryRemove("newRoom", out _);
        Console.WriteLine("OnRemove called");
        tcs.SetResult();
    }
    
    [Test]
    public async Task TestActionCalled()
    {
        _testRoomList.Clear();
        
        var tcs = new TaskCompletionSource();
        var newRoom = new GrpcChatServer.ChatRoom("newRoom");

        newRoom.Handler.OnRemove += () => AddFunctionHandler(tcs);
        
        _testRoomList.TryAdd("newRoom", newRoom);

        newRoom.Enter(_testClient1, TestAction);
        var expected = _testRoomList.Count - 1;

        newRoom.Exit(_testClient1.Peer);
        await tcs.Task;
        
        var afterExit = _testRoomList.Count;
        
        Assert.AreEqual(expected, afterExit);
    }
    
    [Test]
    public async Task TestActionCalledCreated()
    {
        _testRoomList.Clear();
        
        var tcs = new TaskCompletionSource();
        var newRoom = new GrpcChatServer.ChatRoom("newRoom");

        newRoom.Handler.OnRemove += () => AddFunctionHandler(tcs);
        
        _testRoomList.TryAdd("newRoom", newRoom);
        
        var expected = _testRoomList.Count - 1;
        
        await tcs.Task;
        
        var after = _testRoomList.Count;
        
        Assert.AreEqual(expected, after);
    }
}