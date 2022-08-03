using System.Collections.Concurrent;

namespace GrpcChatServer;

public class ChatRoom
{
    public IKillEventHandler KillEventHandler => _killObserver;
    
    public string Name { get; }
    
    private const int QueueSize = 10;
    private const int KillTimeout = 5000;

    private readonly KillObserver _killObserver = new(); // NOTE: 일정 시간 후 채팅방 삭제 로직을 분리해보자.

    private readonly ConcurrentDictionary<string, Client> _clients = new();
    private readonly ConcurrentQueue<MessageData> _previousChats = new();

    public ChatRoom(string chatRoomName)
    {
        Name = chatRoomName;
        
        _killObserver.OnNext(KillTimeout);
    }

    public EnterContext Enter(Client client, Action<MessageData> action)
    {
        _clients.TryAdd(client.Peer, client);
        Console.WriteLine($"Client {Name} entered room {client.Name}.");
        
        _killObserver.Cancel();

        return new EnterContext(client, this, action);
    }

    public void Exit(string peer)
    {
        var currentClient = _clients.Values.First(client => client.Peer.Equals(peer));
        
        _clients.TryRemove(currentClient.Peer, out _);
        
        if(_clients.IsEmpty)
            _killObserver.OnNext(KillTimeout);
    }

    public int ParticipantsCount()
    {
        return _clients.Count;
    }

    public void Broadcast(string peer, string message)
    {
        if (_previousChats.Count >= QueueSize)
        {
            _previousChats.TryDequeue(out _);
        }

        var context = new MessageData(_clients[peer], DateTime.Now, message);

        _previousChats.Enqueue(context);

        foreach (var others in _clients.Where(client => client.Key != peer))
        {
            others.Value.Send(context);
        }
    }

    public void SendPrevChat(string peer)
    {
        if (!_clients.TryGetValue(peer, out var client)) return;

        foreach (var chat in _previousChats)
        {
            client.Send(chat);
        }
    }
}