namespace GrpcChatServer;

public class ServerService
{
    
}
// public override async Task EnterRoom(IAsyncStreamReader<RoomRequest> requestStream,
//         IServerStreamWriter<RoomResponse> responseStream, ServerCallContext context)
//     {
//         ChatRoom? currentRoom = null;
//         var currentClient = _clientList.Values.First(client => client.Peer.Equals(context.Peer));
//         // var room = _chatRoomList.Values.FirstOrDefault(room => room.Name.Equals(roomName));
//         while (await requestStream.MoveNext())
//         {
//             switch (requestStream.Current.RequestCase)
//             {
//                 case RoomRequest.RequestOneofCase.Chat:
//                     if (currentRoom is null)
//                         throw new InvalidOperationException();
//
//                     var chat = requestStream.Current.Chat;
//
//                     var reply = new RoomResponse
//                     {
//                         Chat = new ChatMessage
//                         {
//                             Message = $"[{DateTime.Now:yyyy-MM-dd hh:mm:ss tt}] {currentClient.Name} : {chat.Message}",
//                             RoomName = "useless"
//                         }
//                     };
//
//                     foreach (var writer in currentRoom.StreamList)
//                     {
//                         if (writer.Value != responseStream)
//                         {
//                             await writer.Value.WriteAsync(reply);
//                         }
//                     }
//
//                     break;
//                 
//                 // When client entered room.
//                 // Add streamwriter in streamlist.
//                 // Add client information in _clients.
//                 case RoomRequest.RequestOneofCase.Enter:
//                     var roomName = requestStream.Current.Enter.RoomName;
//                     var room = _chatRoomList.Values.FirstOrDefault(room => room.Name.Equals(roomName));
//
//                     if (room is null)
//                     {
//                         await responseStream.WriteAsync(
//                             new RoomResponse
//                             {
//                                 Enter = new EnterResponse
//                                 {
//                                     Failed = new FailedResponse { Reason = "not Found" }
//                                 }
//                             });
//                         return;
//                     }
//
//                     currentRoom = room;
//                     currentRoom.StreamList.TryAdd(context.Peer, responseStream);
//                     
//                     currentClient.JoinedRoom = currentRoom.Name;
//                     room.Enter(context.Peer, currentClient);
//
//                     Console.WriteLine($"i found! Client : {context.Peer}");
//
//
//                     await responseStream.WriteAsync(
//                         new RoomResponse
//                         {
//                             Enter = new EnterResponse
//                             {
//                                 Success = new RoomInformation
//                                 {
//                                     Name = room.Name
//                                 }
//                             }
//                         }
//                     );
//
//                     break;
//
//                 case RoomRequest.RequestOneofCase.None:
//                 default:
//                     throw new InvalidOperationException();
//             }
//         }
//         // When client quit.
//
//         currentRoom?.Exit(currentClient);
//     }