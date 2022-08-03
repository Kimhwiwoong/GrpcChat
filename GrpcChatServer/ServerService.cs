using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using GrpcChatServer.Exceptions;

namespace GrpcChatServer;

public class ServerService
{
    private readonly ConcurrentDictionary<string, Client> _clientList = new();
    private readonly ConcurrentDictionary<string, ChatRoom> _chatRoomList = new();

    public string Enroll(string peer)
    {
        string nickname;
        Client client;
        
        do
        {
            nickname = GenerateNickname();
            client = new Client(peer, nickname);
        } while (!_clientList.TryAdd(peer, client));

        return nickname;
    }

    private static string GenerateNickname()
    {
        return $"user{new Random().Next(1, int.MaxValue)}";
    }

    public bool TryChangeNick(string newName, string peer)
    {
        var target = _clientList.Values.FirstOrDefault(client => client.Peer.Equals(peer));
        if (target is null)
            return false;

        target.Name = newName;
        return true;
    }

    public bool TryCreateRoom(string name)
    {
        var newRoom = new ChatRoom(name);
        // 으악 순서꼬였다 생성자에 넣으면 안되네
        newRoom.OnRemove += RemoveRoom;
        
        return _chatRoomList.TryAdd(name, newRoom);
    }

    public Dictionary<string, int> ShowRooms()
    {
        var roomList = _chatRoomList.Values
            .ToDictionary(room => room.Name, room => room.ParticipantsCount());

        return roomList;
    }

    public void RemoveRoom(string name)
    {
        _chatRoomList.TryRemove(name, out _);
        Console.WriteLine($"Room {name} removed for no user.");
    }

    public Client FindClient(string peer)
    {
        if (!_clientList.TryGetValue(peer, out var client))
            throw new ClientNotFoundException(peer);
        
        return client;
        //return _clientList.Values.First(client => client.Peer.Equals(peer));
    }

    public ChatRoom FindRoom(string name)
    {
        if (!_chatRoomList.TryGetValue(name, out var room))
            throw new RoomNotFoundException(name);

        return room;
        // var room = _chatRoomList.Values.FirstOrDefault(room => room.Name.Equals(name));
    }

    public bool TryFindRoom(string name, [MaybeNullWhen(false)] out ChatRoom room)
    {
        return _chatRoomList.TryGetValue(name, out room);
        // room = _chatRoomList.Values.FirstOrDefault(room => room.Name.Equals(name));
        // return room is not null;
    }
}