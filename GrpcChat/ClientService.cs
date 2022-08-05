using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcChat.Exceptions;

namespace GrpcChat;

public class ClientService
{
    private readonly ChatGrpc.ChatGrpcClient _client;
    private string? _userNickname;
    
    public ClientService(ChatGrpc.ChatGrpcClient client)
    {
        _client = client;
    }
    
    public void Enroll()
    {
        var request = new Empty();
        var reply = _client.Enroll(request);

        if (reply is null)
        {
            throw new Exception("Enroll reply is null");
        }

        _userNickname = reply.Nickname;
    }
    
    public void CreateRoom(string name)
    {
        var request = new CreateRoomRequest()
        {
            Name = name
        };
        
        var reply = _client.CreateRoom(request);
        
        switch (reply.ResponseCase)
        {
            case SuccessFailResponse.ResponseOneofCase.Success:
                return;
            
            case SuccessFailResponse.ResponseOneofCase.Failed:
                throw new Exception(reply.Failed.Reason);
            
            case SuccessFailResponse.ResponseOneofCase.None:
            default:
                throw new InvalidOperationException("");
        }
        
    }
    
    public void ChangeNickname(string newNickname)
    {
        var request = new ChangeNickRequest()
        {
            NewNickname = newNickname
        };

        var reply = _client.ChangeNick(request);
        
        switch (reply.ResponseCase)
        {
            case SuccessFailResponse.ResponseOneofCase.Success:
                _userNickname = newNickname;
                return;
            
            case SuccessFailResponse.ResponseOneofCase.Failed:
                throw new Exception(reply.Failed.Reason);
            
            case SuccessFailResponse.ResponseOneofCase.None:
            default:
                throw new InvalidOperationException("");
        }

    }
    
    public IEnumerable<string> ShowRooms()
    {
        var request = new Empty();
        var reply = _client.ShowRooms(request);

        return reply.RoomInfos
            .Select(infoResponse => $"{infoResponse.Name} ({infoResponse.ParticipantsCount})")
            .ToList();
    }

    public Room EnterRoom(string roomName)
    {
        var call = _client.EnterRoom();
        var request = new ChatRoomRequest
        {
            Enter = new EnterRoomRequest
            {
                RoomName = roomName
            }
        };
        
        call.RequestStream.WriteAsync(request).Wait();
        if (!call.ResponseStream.MoveNext().Result)
            throw new InvalidOperationException();

        var reply = call.ResponseStream.Current;

        if (reply is { Failed: { } failed })
            throw new EnterRoomException(failed.Reason);
        
        if (reply is not { Enter: { } enter })
            throw new EnterRoomException("Invalid reply: reply is not type enter.");

        return new Room(call);
    }

    public string GetCurrentNickname()
    {
        return _userNickname!;
    }
}
