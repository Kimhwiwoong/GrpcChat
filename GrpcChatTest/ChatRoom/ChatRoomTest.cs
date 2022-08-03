using System.Collections.Concurrent;
using GrpcChatServer;
using NUnit.Framework;

namespace GrpcChatTest.ChatRoom;

public class ChatRoomTest
{
    private readonly ConcurrentDictionary<string, Client> _clients = new();
    private readonly ConcurrentQueue<MessageData> _previousChats = new();
    private GrpcChatServer.ChatRoom _room = null!;
    [SetUp]
    public void SetUp()
    {
        _room = new GrpcChatServer.ChatRoom("Test");
    }
}