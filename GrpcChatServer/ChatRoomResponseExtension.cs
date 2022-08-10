using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcChatServer.Exceptions;

namespace GrpcChatServer;

public static class ChatRoomResponseExtension
{
    public static void SendMessage(this IAsyncStreamWriter<ChatRoomResponse> responseStream,
        MessageData data)
    {
        var uuid = Random.Shared.Next();

        lock (responseStream)
        {
            // Console.WriteLine($"{data.Sender.Peer}: {data.Message} / {uuid} START");
            responseStream.WriteAsync(new ChatRoomResponse
            {
                Chat = new ChatMessageResponse
                {
                    Message = data.Message,
                    Nickname = data.Sender.Name,
                    Time = Timestamp.FromDateTime(data.Time.ToUniversalTime())
                }
            }).Wait();
        }
        
        // Console.WriteLine($"{data.Sender.Peer}: {data.Message} / {uuid} END");
    }
    
    
    // Adapter
    public static void SendFail(this IAsyncStreamWriter<ChatRoomResponse> responseStream, ServerException e)
    {
        lock (responseStream)
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
    }

    public static void SendEnter(this IAsyncStreamWriter<ChatRoomResponse> responseStream, ChatRoom room)
    {
        var messageList = new RepeatedField<ChatMessageResponse>();
        
        foreach (var message in room.GetPrevChats())
        {
            messageList.Add(new ChatMessageResponse
            {
                Message = message.Message,
                Nickname = message.Sender.Name,
                Time = message.Time.ToUniversalTime().ToTimestamp()
            });
        }

        lock (responseStream)
        {
            responseStream.WriteAsync(
                new ChatRoomResponse
                {
                    PrevChats = new PrevChatsResponse
                    {
                        PrevChats = { messageList }
                    }
                }
            ).Wait();
        }
    }
}