using System.Collections.Concurrent;

namespace GrpcChatServer;

public class ChatRoom
{
    public IRemoveEventHandler Handler => _removeObserver;
    
    public string Name { get; }

    private const int QueueSize = 10;
    private const int WaitDurationMilli = 5000;
    
    private readonly ConcurrentDictionary<string, Client> _clients = new();
    private readonly ConcurrentQueue<MessageData> _previousChats = new();

    private readonly RemoveObserver _removeObserver = new();
    
    public EnterContext Enter(Client client, Action<MessageData> onSendHandler)
    {
        _clients.TryAdd(client.Peer, client);

        _removeObserver.Cancel();

        Console.WriteLine($"Client {client.Name} entered room {Name}.");

        return new EnterContext(client, this, onSendHandler);
    }

    public void Exit(string peer)
    {
        if (!_clients.TryGetValue(peer, out var currentClient)) return;
        
        _clients.TryRemove(currentClient.Peer, out _);

        if (_clients.IsEmpty)
        {
            _removeObserver.OnNext(WaitDurationMilli);
        }
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

        var messageData = new MessageData(_clients[peer], DateTime.Now, message);

        _previousChats.Enqueue(messageData);

        var others = _clients
            .Where(clientPair => clientPair.Key != peer)
            .Select(clientPair => clientPair.Value);
        
        foreach (var other in others)
        {
            other.Send(messageData);
        }
    }

    // public void SendPrevChat(string peer)
    // {
    //     if (!_clients.TryGetValue(peer, out var currentClient)) return;
    //
    //     foreach (var chat in _previousChats)
    //     {
    //         currentClient.Send(chat);
    //     }
    // }

    public ConcurrentQueue<MessageData> GetPrevChats()
    {
        return _previousChats;
    }

    public ChatRoom(string chatRoomName)
    {
        Name = chatRoomName;
        _removeObserver.OnNext(WaitDurationMilli);
    }
}