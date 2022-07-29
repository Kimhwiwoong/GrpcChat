using Grpc.Core;

namespace GrpcChat
{
    class GrpcChat
    {
        //private readonly ChatGrpc.ChatGrpcClient _client;   
        public static void Main()
        {
            var channel = new Channel("127.0.0.1:12345", ChannelCredentials.Insecure);
            var client = new ChatGrpc.ChatGrpcClient(channel);
            
            var service = new ClientService(client);
            var controller = new ClientController(service);
            
            controller.Start();
        }
        //
        // public static void Main()
        // {
        //     var channel = new Channel("127.0.0.1:12345", ChannelCredentials.Insecure);
        //     var client = new ChatGrpc.ChatGrpcClient(channel);
        //     
        //     new GrpcChat(client).Start();
        // }
     
        // public GrpcChat(ChatGrpc.ChatGrpcClient client)
        // {
        //     _client = client;
        // }
        //
        // public void Start()
        // {
        //     Setup();
        //     
        //     while (true)
        //     {
        //         try
        //         {
        //             switch (Console.ReadLine())
        //             {
        //                 case "create_room" :
        //                     OnCreateRoom();
        //                     break;
        //                 
        //                 case "show_rooms" :
        //                     OnShowRooms();
        //                     break;
        //                 
        //                 case "change_nickname" :
        //                     OnChangeNickname();
        //                     break;
        //                 
        //                 case "enter_room" : 
        //                     OnEnterRoom().AsTask().Wait();
        //                     break;
        //                 
        //                 case "exit":
        //                     Console.WriteLine("GoodBye");
        //                     continue;
        //
        //                 default:
        //                     Console.WriteLine("Wrong command");
        //                     break;
        //             }
        //         }
        //         catch (Exception e)
        //         {
        //             Console.WriteLine(e.Message);
        //         }
        //     }
        // }
        //
        // private void Setup()
        // {
        //     Enroll();
        //     
        //     Console.WriteLine("______________________________________________");
        //     Console.WriteLine("change_nickname   :   닉변                     ");
        //     Console.WriteLine("create_room       :   방 생성                   ");
        //     Console.WriteLine("show_rooms        :   방 목록 조회               ");
        //     Console.WriteLine("enter_room        :   방 입장                   ");
        //     Console.WriteLine("exit              :   종료                     ");
        //     Console.WriteLine("______________________________________________");
        //     Console.WriteLine($"Your Nickname : {_userNickname}");
        // }
        //
        // private void OnCreateRoom()
        // {
        //     Console.Write("name for room : ");
        //     var roomName = Console.ReadLine();
        //     
        //     while (true)
        //     {
        //         var request = new RoomInformation
        //         {
        //             Name = roomName
        //         };
        //         var reply = _client.CreateRoom(request);
        //         
        //         switch (reply.ResponseCase)
        //         {
        //             case CreateResponse.ResponseOneofCase.Failed :
        //                 Console.WriteLine(reply.Failed.Reason);
        //                 Console.WriteLine("new room name : ");
        //                 roomName = Console.ReadLine();
        //                 continue;
        //
        //             case CreateResponse.ResponseOneofCase.Success :
        //                 Console.WriteLine("Room Created.");
        //                 break;
        //         
        //             case CreateResponse.ResponseOneofCase.None :
        //             default :
        //                 throw new InvalidOperationException();
        //         }
        //         break;
        //     }
        // }
        //
        // private void OnChangeNickname()
        // {
        //     Console.Write("nickname to change : ");
        //     var newNickname = Console.ReadLine();
        //
        //     while (string.IsNullOrWhiteSpace(newNickname))
        //     {
        //         Console.WriteLine("try again.");
        //         Console.Write("nickname to change : ");
        //         newNickname = Console.ReadLine();
        //     }
        //     
        //     var request = new ChangeNickRequest()
        //     {
        //         OldName = _userNickname,
        //         NewName = newNickname
        //     };
        //     
        //     while (true)
        //     {
        //         var reply = _client.ChangeNick(request);
        //         if (reply.Success is not null)
        //         {
        //             Console.WriteLine(
        //                 "Your nickname changed.\n" +
        //                 $"before : {request.OldName}\n" +
        //                 $"after : {request.NewName}"
        //             );
        //             _userNickname = request.NewName!;
        //             break;
        //         }
        //
        //         Console.WriteLine("That nickname already exist.");
        //         Console.Write("Nickname to use : ");
        //         request.NewName = Console.ReadLine();
        //     }
        // }
        //
        // private void OnShowRooms()
        // {
        //     var request = new Empty();
        //     var reply = _client.ShowRooms(request);
        //     
        //     foreach (var names in reply.Names)
        //     {
        //         Console.WriteLine(names);
        //     }
        // }
        //
        // private async ValueTask OnEnterRoom()
        // {
        //     Console.Write("room name to enter : ");
        //     var requestRoomName = Console.ReadLine();
        //     if (requestRoomName is null)
        //         throw new Exception("Room Name is null!");
        //     
        //     var call = _client.EnterRoom();
        //     await call.RequestStream.WriteAsync(new RoomRequest
        //     {
        //         Enter = new EnterRequest
        //         {
        //             RoomName = requestRoomName
        //         }
        //     });
        //
        //     if (!await call.ResponseStream.MoveNext())
        //         throw new Exception("Server dead");
        //
        //     if (call.ResponseStream.Current is not { Enter: {} enter })
        //         throw new Exception("Wrong message from server");
        //     
        //     var roomName = OnEnterResponse(enter);
        //     
        //     var readingTask = Task.Run(async () =>
        //     {
        //         while (await call.ResponseStream.MoveNext())
        //         {
        //             var response = call.ResponseStream.Current;
        //             switch (response.ResponseCase)
        //             {
        //                 case RoomResponse.ResponseOneofCase.Chat:
        //                     OnChatMessage(response.Chat);
        //                     break;
        //
        //                 case RoomResponse.ResponseOneofCase.Enter:
        //                 case RoomResponse.ResponseOneofCase.None:
        //                 default:
        //                     throw new InvalidOperationException();
        //             }
        //         }
        //     });
        //
        //     while (true)
        //     {
        //         var line = Console.ReadLine();
        //         if (line == "quit")
        //             break;
        //
        //         await call.RequestStream.WriteAsync(new RoomRequest
        //         {
        //             Chat = new ChatMessage
        //             {
        //                 Message = line,
        //                 RoomName = requestRoomName
        //             }
        //         });
        //     }
        //
        //     await call.RequestStream.CompleteAsync();
        //     await readingTask;
        // }
        //
        // private void OnChatMessage(ChatMessage message)
        // {
        //     Console.WriteLine(message.Message);
        // }
        //
        // private void Enroll()
        // {
        //     var request = new Empty();
        //     var reply = _client.Enroll(request);
        //
        //     if (reply is null)
        //     {
        //         throw new Exception("Enroll reply is null");
        //     }
        //
        //     _userNickname = reply.Nickname;
        // }
        //
        // private string OnEnterResponse(EnterResponse response)
        // {
        //     switch (response.ResponseCase)
        //     {
        //         case EnterResponse.ResponseOneofCase.Success:
        //             return response.Success.Name;
        //             
        //         case EnterResponse.ResponseOneofCase.Failed:
        //             throw new Exception(response.Failed.Reason);
        //         
        //         case EnterResponse.ResponseOneofCase.None:
        //         default:
        //             throw new InvalidOperationException();
        //     }
        // }
        //
        // private string GetCurrentNickname()
        // {
        //     return _userNickname;
        // }

        // public static void Main2(string[] args)
        // {
        //     var channel = new Channel("127.0.0.1:12345", ChannelCredentials.Insecure);
        //     var client = new ChatGrpc.ChatGrpcClient(channel);
        //
        //     
        //     
        //     while (true)
        //     {
        //         switch (Console.ReadLine())
        //         {
        //             case "create_room" :
        //                 Console.Write("name for room : ");
        //                 var roomName = Console.ReadLine();
        //                 
        //                 while (string.IsNullOrWhiteSpace(roomName))
        //                 {
        //                     Console.WriteLine("try again.");
        //                     Console.Write("name for room : ");
        //                     roomName = Console.ReadLine();
        //                 }
        //                 
        //                 CreateRoom(client, roomName ?? throw new InvalidOperationException());
        //                 break;
        //             
        //             case "show_rooms" :
        //                 ShowRooms(client);
        //                 break;
        //             
        //             case "change_nickname" :
        //                 Console.Write("nickname to change : ");
        //                 var newNickname = Console.ReadLine();
        //                 while (string.IsNullOrWhiteSpace(newNickname))
        //                 {
        //                     Console.WriteLine("try again.");
        //                     Console.Write("nickname to change : ");
        //                     newNickname = Console.ReadLine();
        //                 }
        //
        //                 ChangeName(client, newNickname ?? throw new InvalidOperationException());
        //                 break;
        //             
        //             case "enter_room" :
        //                 Console.Write("room name to enter : ");
        //                 var roomToEnter = Console.ReadLine();
        //                 if (roomToEnter != null)
        //                 {
        //                     EnterRoom(client, roomToEnter).Wait();
        //                 }
        //                 else
        //                 {
        //                     throw new Exception("There's an exception by roomToEnter");
        //                 }
        //
        //                 break;
        //         }
        //     }
        // }    

        // private static async Task OnEnterResponse(EnterResponse response, string roomName)
        // {
        //     switch (response.ResponseCase)
        //     {
        //         case EnterResponse.ResponseOneofCase.Success:
        //             var request = new RoomRequest
        //             {
        //                 Chat = new ChatMessage
        //                 {
        //                     Message = Console.ReadLine(),
        //                     RoomName = roomName
        //                 }
        //             };
        //             break;
        //         
        //         case EnterResponse.ResponseOneofCase.Failed:
        //             Console.WriteLine(response.Failed.Reason);
        //             Console.Write("retry : ");
        //             break;
        //         
        //         case EnterResponse.ResponseOneofCase.None:
        //         default:
        //             throw new InvalidOperationException();
        //     }
        // }

        // private static async Task ClientChat(ChatGrpc.ChatGrpcClient client, string roomName)
        // {
        //     var request = new ChatMessage
        //     {
        //         Message = $" {_userNickname} : Joined chat",
        //         RoomName = roomName
        //     };
        //     using var call = client.Chat();
        //     
        //     var responseReaderTask = Task.Run(async () =>
        //     {
        //         while (await call.ResponseStream.MoveNext())
        //         {
        //             var reply = call.ResponseStream.Current;
        //             Console.WriteLine(reply.Message);
        //         }
        //     });
        //     
        //     //await call.RequestStream.WriteAsync(request);
        //     
        //     Console.WriteLine("______________________________________________");
        //     Console.WriteLine("quit  :   채팅방 나가기");
        //     Console.WriteLine("______________________________________________");
        //
        //     var chat = Console.ReadLine();
        //     while (!string.Equals(chat, "quit", StringComparison.OrdinalIgnoreCase))
        //     {
        //         request.Message = $" {_userNickname} : {chat}";
        //         await call.RequestStream.WriteAsync(request);
        //         chat = Console.ReadLine();
        //     }
        //
        //     await call.RequestStream.CompleteAsync();
        //     await responseReaderTask;
        // }
    }
}

