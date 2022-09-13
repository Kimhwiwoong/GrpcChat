using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcChat.Exceptions;
using GrpcChatProto;

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
    
    public IEnumerable<IRoomInformation> ShowRooms()
    {
        var request = new Empty();
        var reply = _client.ShowRooms(request);

        return reply.RoomInfos;
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

        var response = call.ResponseStream.Current;

        if (response is { Failed: { } failed })
            throw new EnterRoomException(failed.Reason);
        
        if (response is not { PrevChats: { } })
            throw new EnterRoomException("Invalid reply: reply is not type enter.");
        
        // 이부분에서 response로 받은 prevChats 처리
        var prevChats = response.PrevChats.PrevChats
            .Select(prevChatResponse =>
                $"[{prevChatResponse.Time.ToDateTime().ToLocalTime():yyyy-M-d dddd hh:mm:ss}] " +
                $"{prevChatResponse.Nickname} : {prevChatResponse.Message}")
            .ToList();
        
        
        return new Room(call, prevChats);
    }

    // public IEnumerable<string> GetPrevChats()
    // {
    //     var request = new Empty();
    //     // var request = new PrevChatsRequest
    //     // {
    //     //     RoomName = roomName
    //     // };
    //     var reply = _client.SendPrevChats(request);
    //
    //     return reply.PrevChats
    //         .Select(prevChatResponse =>
    //             $"[{prevChatResponse.Time.ToDateTime().ToLocalTime()}] " +
    //             $"{prevChatResponse.Nickname} : {prevChatResponse.Message}")
    //         .ToList();
    // }

    public string GetCurrentNickname()
    {
        return _userNickname!;
    }
}
