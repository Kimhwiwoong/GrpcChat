using System.Collections.Concurrent;

namespace GrpcChatServer;

public class ChatRoom
{
    private const int QueueSize = 10;
    private readonly ConcurrentDictionary<string, Client> _clients = new();
    //private readonly ConcurrentQueue<string> _previousChats = new();
    private readonly ConcurrentQueue<Tuple<string, string>> _previousChats = new();
    public string Name { get; }
    

    public EnterContext Enter(Client client, Action<MessageContext> action)
    {
        _clients.TryAdd(client.Peer, client);
        Console.WriteLine($"Client {Name} entered room {client.Name}.");
        
        return new EnterContext(client, this, action);
    }
    
    public void Exit(string peer)
    {
        var currentClient = _clients.Values.First(client => client.Peer.Equals(peer));

        _clients.TryRemove(currentClient.Peer, out _);
        
    }

    public string GetCurrentClientName(string peer)
    {
        var client = _clients.Values.First(client => client.Peer.Equals(peer));
        
        return client.Name;
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
        
        // _previousChats.Enqueue(message);
        _previousChats.Enqueue(new Tuple<string, string>(peer, message));
        
        var context = new MessageContext(_clients[peer], DateTime.Now, message);

        foreach (var others in _clients.Where(client => client.Key != peer))
        {
            others.Value.Send(context);
        }
    }

    public void SendPrevChat(string peer)
    {
        var currentClient = _clients.Values.First(client => client.Peer.Equals(peer));
        
        
        foreach (var chat in _previousChats)
        {
            // currentClient.Send(new MessageContext(_clients[peer], DateTime.Now, chat));
            currentClient.Send(new MessageContext(_clients[chat.Item1], DateTime.Now, chat.Item2));
        }
    }

    public ChatRoom(string chatRoomName)
    {
        Name = chatRoomName;
    }
}