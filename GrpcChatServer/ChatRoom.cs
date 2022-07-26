using System.Collections.Concurrent;

namespace GrpcChatServer;

public class ChatRoom
{
    private readonly ConcurrentDictionary<string, Client> _clients = new();
    public string Name { get; }
    
    public void Enter(string peer, Client client)
    {
        // lock (_participantsLock)
        // {
        //     ParticipantsCount++;    
        // }
        _clients.TryAdd(peer, client);
        //Interlocked.Increment(ref _participantsCount);
    }
    
    public void Exit(string peer)
    {
        var currentClient = _clients.Values.First(client => client.Peer.Equals(peer));

        _clients.TryRemove(currentClient.Peer, out var clientRemover);
        
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
        foreach (var others in _clients.Where(client => client.Key != peer))
        {
            others.Value.Send(message);
        }
    }

    public ChatRoom(string chatRoomName)
    {
        Name = chatRoomName;
    }
}