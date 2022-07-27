using System.Collections.Concurrent;

namespace GrpcChatServer;

public class ServerService 
{
    private readonly ConcurrentDictionary<string, Client> _clientList = new();
    private readonly ConcurrentDictionary<string, ChatRoom> _chatRoomList = new();

    private bool NicknameAlready(string target)
    {
        return _clientList.Values.Any(sameName => sameName.Name.Equals(target));
    }
    
    private bool RoomNameAlready(string target)
    {
        return _chatRoomList.Any(sameName => sameName.Key.Equals(target));
    }

    public string Enroll(string peer)
    {
        // When nickname is unique, add client to list and return client's nickname 
        var nickname = $"user{new Random().Next(1, int.MaxValue)}";
        while (NicknameAlready(nickname))
        {
            nickname = $"user{new Random().Next(1, int.MaxValue)}";
        }
        var client = new Client(peer, nickname);

        _clientList.TryAdd(nickname, client);

        return nickname;
    }
    
    public bool ChangeNick(string newName, string peer)
    {
        if (NicknameAlready(newName))
        {
            // Fail -> return false
            return false;
        }
        // Success -> change nickname and return true
        var target = _clientList.Values.First(client => client.Peer.Equals(peer));
        target.Name = newName;
        
        return true;
    }

    public bool CreateRoom(string name)
    {
        if (RoomNameAlready(name))
        {
            // Fail -> return false
            return false;
        }
        // Success -> create room and return true
        var newRoom = new ChatRoom(name);
        _chatRoomList.TryAdd(name, newRoom);
        
        return true;
    }

    public IEnumerable<string> ShowRooms()
    {
        var names = _chatRoomList.Values.Select(room => $"{room.Name} ({room.ParticipantsCount()})");
    
        // if (ChatRoomManger.TryCreateRoom("", out var room))
        // {
        //     room
        // }
        
        return names;
    }

    public void JoinClientToRoom(ChatRoom room, string peer)
    {
        var currentClient = _clientList.Values.First(client => client.Peer.Equals(peer));

        room.Enter(peer, currentClient);
        
        Console.WriteLine($"Client {currentClient.Name} entered room {room.Name}.");
    }

    public Client GetCurrentClient(string peer)
    {
        return _clientList.Values.First(client => client.Peer.Equals(peer));
    }

    public ChatRoom? GetCurrentRoom(string name)
    {
        return _chatRoomList.Values.FirstOrDefault(room => room.Name.Equals(name));
    }
}