using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcChatServer.Exceptions;

namespace GrpcChatServer;

public static class ChatRoomResponseExtension
{
    public static void SendMessage(this IAsyncStreamWriter<ChatRoomResponse> responseStream,
        MessageContext context)
    {
        responseStream.WriteAsync(new ChatRoomResponse
        {
            Chat = new ChatMessageResponse
            {
                Message = context.Message,
                Nickname = context.Sender.Name,
                Time = Timestamp.FromDateTime(context.Time.ToUniversalTime())
            }
        }).Wait();
    }
    
    
    // Adaptor
    public static void SendFail(this IAsyncStreamWriter<ChatRoomResponse> responseStream, ServerException e)
    {
        responseStream.WriteAsync(
            new ChatRoomResponse
            {
                Failed = new FailedResponse
                {
                    Reason = e.Message
                }
            }
        ).Wait();
    }

    public static void SendEnter(this IAsyncStreamWriter<ChatRoomResponse> responseStream)
    {
        responseStream.WriteAsync(
            new ChatRoomResponse
            {
                Enter = new SuccessFailResponse
                {
                    Success = new Empty()
                }
            }
        ).Wait();
    }
}