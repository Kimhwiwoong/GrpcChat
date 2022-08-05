using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcChat;

namespace GrpcChatTest.Utility;

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
            return null!;
        }

        var moveNext = await call.ResponseStream.MoveNext(token);
        if (!moveNext)
        {
            Console.WriteLine("MoveNext is false");
            return null!;
            // throw new Exception("MoveNext is false");   
        }

        var reply = call.ResponseStream.Current;

        if (reply.ResponseCase != ChatRoomResponse.ResponseOneofCase.Enter)
        {
            Console.WriteLine(reply.ResponseCase);
        }

        return call;
    }

    public static async void CreateRoom(ChatGrpc.ChatGrpcClient client, string roomName)
    {
        var request = new CreateRoomRequest
        {
            Name = roomName
        };

        await client.CreateRoomAsync(request);
    }

    public static void Enroll(ChatGrpc.ChatGrpcClient client)
    {
        var request = new Empty();
        client.Enroll(request);
    }

    
    
    
    // 얘는 clientService용인데??
    public static void EnrollAndCreateRoom(string name, ClientService clientService)
    {
        clientService.Enroll();
        clientService.CreateRoom(name);
    }
}