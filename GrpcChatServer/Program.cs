using Grpc.Core;

namespace GrpcChatServer;

internal class GrpcChatServerImpl : ChatGrpc.ChatGrpcBase
{
    private const int Port = 12345;

    // nicknamealready roomnamealready 공백검사 포함
    // private bool NicknameAlready(string target)
    // {
    //     return _clientList.Any(sameName => sameName.Key.Equals(target));
    // }
    // private bool RoomNameAlready(string target)
    // {
    //     return _chatRoomList.Any(sameName => sameName.Key.Equals(target));
    // }
    //
    // public override Task<EnrollClient> Enroll(Empty request, ServerCallContext context)
    // {
    //     var nickname = $"user{new Random().Next(1, int.MaxValue)}";
    //     while (NicknameAlready(nickname))
    //     {
    //         nickname = $"user{new Random().Next(1, int.MaxValue)}";
    //     }
    //
    //     var reply = new EnrollClient
    //     {
    //         Nickname = nickname
    //     };
    //
    //     var client = new Client(context.Peer, nickname);
    //
    //     _clientList.TryAdd(nickname, client);
    //     Console.WriteLine("user enroll success");
    //     
    //     return Task.FromResult(reply);
    // }
    //
    // public override Task<ChangeNickResponse> ChangeNick(Nickname request, ServerCallContext context)
    // {
    //     var reply = new ChangeNickResponse();
    //     // 여기에 공백 검사까지 낑가넣어
    //     if (NicknameAlready(request.NewName))
    //     {
    //         reply.Failed = new FailedResponse
    //         {
    //             Reason = "already exist"
    //         };
    //         return Task.FromResult(reply);
    //     }
    //
    //     var target = _clientList.Values.First(client => client.Name.Equals(request.OldName));
    //     target.Name = request.NewName;
    //
    //     reply.Success = request;
    //     var infoMsg = $"User \"{request.OldName}\" changed nickname to \"{request.NewName}\"";
    //     Console.WriteLine(infoMsg);
    //
    //     return Task.FromResult(reply);
    // }
    //
    // public override Task<CreateResponse> CreateRoom(RoomInformation request, ServerCallContext context)
    // {
    //     var reply = new CreateResponse();
    //     
    //     if (RoomNameAlready(request.Name))
    //     {
    //         reply.Failed = new FailedResponse
    //         {
    //             Reason = "Room name already exist"
    //         };
    //         return Task.FromResult(reply);
    //     }
    //     
    //     reply.Success = new Empty();
    //     var newRoom = new ChatRoom(request.Name);
    //     _chatRoomList.TryAdd(request.Name, newRoom);
    //
    //     return Task.FromResult(reply);
    // }
    //
    // public override Task<Rooms> ShowRooms(Empty request, ServerCallContext context)
    // {
    //     var names = _chatRoomList.Values.Select(room => $"{room.Name} ({room.ParticipantsCount()})");
    //     var reply = new Rooms()
    //     {
    //         Names = { names }
    //     };
    //
    //     return Task.FromResult(reply);
    // }
    //
    // private RoomResponse GetChatResponse(ChatRoom room, IAsyncStreamReader<RoomRequest> requestStream, ServerCallContext context)
    // {
    //     var currentClient = _clientList.Values.First(client => client.Peer.Equals(context.Peer));
    //     
    //     if (room is null)
    //         throw new InvalidOperationException();
    //
    //     var chat = requestStream.Current.Chat;
    //     
    //     var reply = new RoomResponse
    //     {
    //         Chat = new ChatMessage
    //         {
    //             Message = $"[{DateTime.Now:yyyy-MM-dd hh:mm:ss tt}] {currentClient.Name} : {chat.Message}",
    //             RoomName = "useless"
    //         }
    //     };
    //     return reply;
    // }
    //
    // private async void NoRoomResponseSender(IAsyncStreamWriter<RoomResponse> responseStream)
    // {
    //     await responseStream.WriteAsync(
    //         new RoomResponse
    //         {
    //             Enter = new EnterResponse
    //             {
    //                 Failed = new FailedResponse { Reason = "not Found" }
    //             }
    //         });
    // }
    //
    // private async void EnteredResponseSender(ChatRoom room, IServerStreamWriter<RoomResponse> responseStream)
    // {
    //     await responseStream.WriteAsync(
    //         new RoomResponse
    //         {
    //             Enter = new EnterResponse
    //             {
    //                 Success = new RoomInformation
    //                 {
    //                     Name = room.Name
    //                 }
    //             }
    //         }
    //     );
    // }
    //
    // private void JoinClientToRoom(ChatRoom room, IServerStreamWriter<RoomResponse> responseStream, ServerCallContext context)
    // {
    //     var currentClient = _clientList.Values.First(client => client.Peer.Equals(context.Peer));
    //     room.StreamList.TryAdd(context.Peer, responseStream);
    //     currentClient.JoinedRoom = room.Name;
    //     room.Enter(context.Peer, currentClient);
    //     Console.WriteLine($"Client {currentClient.Name} entered room {room.Name}.");
    // }
    //
    // public override async Task EnterRoom(IAsyncStreamReader<RoomRequest> requestStream,
    //     IServerStreamWriter<RoomResponse> responseStream, ServerCallContext context)
    // {
    //     ChatRoom? currentRoom = null;
    //     var currentClient = _clientList.Values.First(client => client.Peer.Equals(context.Peer));
    //     
    //     while (await requestStream.MoveNext())
    //     {
    //         switch (requestStream.Current.RequestCase)
    //         {
    //             case RoomRequest.RequestOneofCase.Chat:
    //                 // if (currentRoom is null)
    //                 //     throw new InvalidOperationException();
    //                 //
    //                 // var chat = requestStream.Current.Chat;
    //                 //
    //                 // var reply = new RoomResponse
    //                 // {
    //                 //     Chat = new ChatMessage
    //                 //     {
    //                 //         Message = $"[{DateTime.Now:yyyy-MM-dd hh:mm:ss tt}] {currentClient.Name} : {chat.Message}",
    //                 //         RoomName = "useless"
    //                 //     }
    //                 // };
    //                 
    //                 var reply = GetChatResponse(currentRoom!, requestStream, context);
    //                 
    //                 foreach (var writer in currentRoom!.StreamList)
    //                 {
    //                     if (writer.Value != responseStream)
    //                     {
    //                         await writer.Value.WriteAsync(reply);
    //                     }
    //                 }
    //
    //                 break;
    //
    //             case RoomRequest.RequestOneofCase.Enter:
    //                 var roomName = requestStream.Current.Enter.RoomName;
    //                 var room = _chatRoomList.Values.FirstOrDefault(room => room.Name.Equals(roomName));
    //                 currentRoom = room;
    //                 
    //                 if (room is null)
    //                 {
    //                     NoRoomResponseSender(responseStream);
    //                 }
    //                 JoinClientToRoom(currentRoom!, responseStream, context);
    //                 EnteredResponseSender(room!, responseStream);
    //                 //JoinClientToRoom(room, client, responseStream)
    //                 //EnteredResponseSender(room, responseStream)
    //                 
    //                  // if (room is null)
    //                  // {
    //                  //     await responseStream.WriteAsync(
    //                  //         new RoomResponse
    //                  //         {
    //                  //             Enter = new EnterResponse
    //                  //             {
    //                  //                 Failed = new FailedResponse { Reason = "not Found" }
    //                  //             }
    //                  //         });
    //                  //     return;
    //                  // }
    //                  //
    //                  // currentRoom = room;
    //                  // currentRoom.StreamList.TryAdd(context.Peer, responseStream);
    //                  //
    //                  // currentClient.JoinedRoom = currentRoom.Name;
    //                  // room.Enter(context.Peer, currentClient);
    //                  //
    //                  // Console.WriteLine($"i found! Client : {context.Peer}");
    //                  //
    //                  //
    //                  // await responseStream.WriteAsync(
    //                  //     new RoomResponse
    //                  //     {
    //                  //         Enter = new EnterResponse
    //                  //         {
    //                  //             Success = new RoomInformation
    //                  //             {
    //                  //                 Name = room.Name
    //                  //             }
    //                  //         }
    //                  //     }
    //                  // );
    //
    //                 break;
    //
    //             case RoomRequest.RequestOneofCase.None:
    //             default:
    //                 throw new InvalidOperationException();
    //         }
    //     }
    //     // When client quit.
    //
    //     currentRoom?.Exit(context.Peer);
    // }

    public static void Main(string[] args)
    {
        var server = new Server
        {
            Services = { ChatGrpc.BindService(new ServerController()) },
            Ports = { new ServerPort("127.0.0.1", Port, ServerCredentials.Insecure) }
        };
        server.Start();

        Console.WriteLine("Greeter server listening on port " + Port);
        Console.WriteLine("Press any key to stop the server...");
        Console.ReadKey();

        server.ShutdownAsync().Wait();
    }
}