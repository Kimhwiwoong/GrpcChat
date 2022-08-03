using System.Collections.Concurrent;

namespace GrpcChatServer;

public class ServerService
{
    // NOTE: IReadonlyDictionary를 써보자.
    public IReadOnlyDictionary<string, Client> Clients => _clients;
    public IReadOnlyDictionary<string, ChatRoom> ChatRooms => _chatRooms;

    private readonly ConcurrentDictionary<string, Client> _clients = new();
    private readonly ConcurrentDictionary<string, ChatRoom> _chatRooms = new();

    public string Enroll(string peer)
    {
        string nickname;
        Client client;
        
        do
        {
            nickname = GenerateNickname();
            client = new Client(peer, nickname);
        } while (!_clients.TryAdd(peer, client));

        return nickname;
    }

    public bool TryChangeNick(string newName, string peer)
    {
        var target = _clients.Values.FirstOrDefault(client => client.Peer.Equals(peer));
        if (target is null)
            return false;

        target.Name = newName;
        return true;
    }

    public bool TryCreateRoom(string name)
    {
        var newRoom = new ChatRoom(name);
        newRoom.KillEventHandler.OnKilled += () => RemoveRoom(name);

        return _chatRooms.TryAdd(name, newRoom);
    }

    public void RemoveRoom(string name)
    {
        _chatRooms.TryRemove(name, out _);
        Console.WriteLine($"Room {name} removed for no user.");
    }
    
    private static string GenerateNickname()
    {
        return $"user{new Random().Next(1, int.MaxValue)}";
    }
}