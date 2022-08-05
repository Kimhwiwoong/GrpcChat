using Grpc.Core;

namespace GrpcChat;

public sealed class Room : IDisposable
{
    public event Action<string>? OnMessage;
    
    private readonly AsyncDuplexStreamingCall<ChatRoomRequest, ChatRoomResponse> _call;
    
    private readonly Task _readingTask;
    
    public Room(AsyncDuplexStreamingCall<ChatRoomRequest, ChatRoomResponse> call)
    {
        _call = call;
        _readingTask = ReadAsync();
    }

    public void SendMessage(string commands)
    {
        var request = new ChatRoomRequest
        {
            Chat = new ChatMessageRequest()
            {
                Message = commands
            }
        };
        
        _call.RequestStream.WriteAsync(request).Wait();
    }

    public void Exit()
    {
        Dispose();
    }
    
    private async Task ReadAsync()
    {
        while (await _call.ResponseStream.MoveNext())
        {
            if (_call.ResponseStream.Current.ResponseCase is not ChatRoomResponse.ResponseOneofCase.Failed)
            {
                var chatResponse = _call.ResponseStream.Current.Chat;

                var utcDateTime = chatResponse.Time.ToDateTime();
                var localTime = utcDateTime.ToLocalTime();

                OnMessage?.Invoke($"[{localTime:yyyy-M-d dddd hh:mm:ss}] {chatResponse.Nickname} : {chatResponse.Message}");
                continue;
            }
            OnMessage?.Invoke(_call.ResponseStream.Current.Failed.Reason);
        }
    }

    public void Dispose()
    {
        _call.RequestStream.CompleteAsync().Wait();
        _readingTask.Wait();
        
        _call.Dispose();
        _readingTask.Dispose();
    }
}