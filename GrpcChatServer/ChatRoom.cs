using System.Collections.Concurrent;
using Grpc.Core;

namespace GrpcChatServer;

public class ChatRoom
{
    private readonly ConcurrentDictionary<string, Client> _clients = new();
    public readonly ConcurrentDictionary<string, IServerStreamWriter<RoomResponse>> StreamList = new();

    public string Name { get; }

    public void Enter(string peer, Client client)
    {
        // lock (_participantsLock)
        // {
        //     ParticipantsCount++;    
        // }
        // Testing.
        _clients.TryAdd(peer, client);
        //Interlocked.Increment(ref _participantsCount);
    }

    public void Exit(Client currentClient)
    {
        currentClient.JoinedRoom = "";
        
        IServerStreamWriter<RoomResponse>? streamRemover;
        Client? clientRemover;
        
        StreamList.TryRemove(currentClient.Peer, out streamRemover);
        _clients.TryRemove(currentClient.Name, out clientRemover);
    }

    public int ParticipantsCount()
    {
        return _clients.Count;
    }

    public ChatRoom(string chatRoomName)
    {
        Name = chatRoomName;
    }
}