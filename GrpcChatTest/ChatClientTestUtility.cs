using Grpc.Core;
using NUnit.Framework;

namespace GrpcChatTest;

public static class ChatClientTestUtility
{
    public static async Task<AsyncDuplexStreamingCall<ChatRoomRequest, ChatRoomResponse>> Enter(ChatGrpc.ChatGrpcClient client, string roomName, CancellationToken token = default)
    {
        var call = client.EnterRoom(cancellationToken: token);
     
        var request = new ChatRoomRequest
        {
            Enter = new EnterRoomRequest
            {
                RoomName = roomName
            }
        };

        try
        {
            // NOTE: GRPC issue - NEVER pass token to WriteAsync.
            await call!.RequestStream.WriteAsync(request);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }

        var moveNext = await call.ResponseStream.MoveNext(token);
        if (!moveNext)
        {
            Console.WriteLine("Movenext is false");
            return null;
            // throw new Exception("MoveNext is false");   
        }

        var reply = call.ResponseStream.Current;

        if (reply.ResponseCase != ChatRoomResponse.ResponseOneofCase.Enter)
            throw new Exception("Wrong response on EnterRequest");

        return call;
    } 
}