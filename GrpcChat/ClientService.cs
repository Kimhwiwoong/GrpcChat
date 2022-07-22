using System.Text;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace GrpcChat;

public class ClientService
{
    private readonly ChatGrpc.ChatGrpcClient _client;
    private static string? _userNickname;
    public ClientService(ChatGrpc.ChatGrpcClient client)
    {
        _client = client;
    }
    
    // 로그인한다.
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
    
    // 방을 만든다.
    public void CreateRoom(string name)
    {
        var request = new RoomInformation
        {
            Name = name
        };
        
        var reply = _client.CreateRoom(request);
        
        switch (reply.ResponseCase)
        {
            case CreateResponse.ResponseOneofCase.Success:
                return;
            
            case CreateResponse.ResponseOneofCase.Failed:
                throw new Exception(reply.Failed.Reason);
            
            case CreateResponse.ResponseOneofCase.None:
            default:
                throw new InvalidOperationException("");
        }
        
    }
    
    // 닉네임을 바꾼다.
    public void ChangeNickname(string newName)
    {
        var request = new Nickname
        {
            OldName = _userNickname,
            NewName = newName
        };

        var reply = _client.ChangeNick(request);
        
        switch (reply.ResponseCase)
        {
            case ChangeNickResponse.ResponseOneofCase.Success:
                return;
            
            case ChangeNickResponse.ResponseOneofCase.Failed:
                throw new Exception(reply.Failed.Reason);
            
            case ChangeNickResponse.ResponseOneofCase.None:
            default:
                throw new InvalidOperationException("");
        }

    }
    
    // 방 목록을 본다
    public IEnumerable<string> ShowRooms()
    {
        var request = new Empty();
        var reply = _client.ShowRooms(request);

        return reply.Names;
    }

    // 방에 들어간다.
    public Room EnterRoom(string roomName)
    {
        var call = _client.EnterRoom();
        var request = new RoomRequest
        {
            Enter = new EnterRequest
            {
                RoomName = roomName
            }
        };
        call.RequestStream.WriteAsync(request).Wait();
        
        if (!call.ResponseStream.MoveNext().Result)
            throw new InvalidOperationException();

        var reply = call.ResponseStream.Current;
        if (reply is not { Enter: { } enter })
            throw new InvalidOperationException();

        switch (enter.ResponseCase)
        {
            case EnterResponse.ResponseOneofCase.Success:
                return new Room(call);

            case EnterResponse.ResponseOneofCase.Failed:
                throw new Exception(enter.Failed.Reason);
        }

        throw new InvalidOperationException();
    }
    
    
    // 닉네임을 반환한다.
    public string GetCurrentNickname()
    {
        return _userNickname!;
    }
}

public class Room
{
    private readonly AsyncDuplexStreamingCall<RoomRequest, RoomResponse> _call;

    // 뭔가 메세지가 왔다.
    public event Action<string>? OnMessage;
    
    private readonly Task _readingTask;
    
    public Room(AsyncDuplexStreamingCall<RoomRequest, RoomResponse> call)
    {
        _call = call;
        _readingTask = ReadAsync();
    }
    
    private async Task ReadAsync()
    {
        while (await _call.ResponseStream.MoveNext())
        {
            OnMessage?.Invoke(_call.ResponseStream.Current.Chat.Message);
        }
    }

    // 메세지를 보낸다.
    public void SendMessage(string commands)
    {
        var request = new RoomRequest
        {
            Chat = new ChatMessage
            {
                Message = commands,
                RoomName = ""
            }
        };
        
        _call.RequestStream.WriteAsync(request).Wait();
    }

    // 방에서 나간다.
    public void Exit()
    {
        _call.RequestStream.CompleteAsync().Wait();
        _readingTask.Wait();
    }
}