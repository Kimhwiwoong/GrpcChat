using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcChatServer.Exceptions;

namespace GrpcChatServer;

public class ServerController : ChatGrpc.ChatGrpcBase
{
    private readonly ServerService _serverService;

    public ServerController(ServerService serverService)
    {
        _serverService = serverService;
    }

    public override Task<EnrollResponse> Enroll(Empty request, ServerCallContext context)
    {
        var nickname = _serverService.Enroll(context.Peer);
        var reply = new EnrollResponse()
        {
            Nickname = nickname
        };

        return Task.FromResult(reply);
    }

    public override Task<SuccessFailResponse> ChangeNick(ChangeNickRequest request, ServerCallContext context)
    {
        var reply = new SuccessFailResponse();
        if (!_serverService.TryChangeNick(request.NewNickname, context.Peer))
        {
            reply.Failed = new FailedResponse
            {
                Reason = "already exist"
            };
            return Task.FromResult(reply);
        }

        reply.Success = new Empty();
        var infoMsg = $"Peer\"{context.Peer}\" changed nickname to \"{request.NewNickname}\"";
        Console.WriteLine(infoMsg);

        return Task.FromResult(reply);
    }

    public override Task<SuccessFailResponse> CreateRoom(CreateRoomRequest request, ServerCallContext context)
    {
        var reply = new SuccessFailResponse();

        if (!_serverService.TryCreateRoom(request.Name))
        {
            reply.Failed = new FailedResponse
            {
                Reason = "Room name already exist"
            };
            return Task.FromResult(reply);
        }

        reply.Success = new Empty();
        
        return Task.FromResult(reply);
    }

    public override Task<ShowRoomsResponse> ShowRooms(Empty request, ServerCallContext context)
    {
        var rooms = _serverService.ShowRooms();

        var infoList = new RepeatedField<RoomInformationResponse>();

        foreach (var room in rooms)
        {
            infoList.Add(new RoomInformationResponse
            {
                Name = room.Key,
                ParticipantsCount = room.Value
            });
        }

        var reply = new ShowRoomsResponse()
        {
            RoomInfos = { infoList }
        };

        return Task.FromResult(reply);
    }

    public override async Task EnterRoom(IAsyncStreamReader<ChatRoomRequest> requestStream,
        IServerStreamWriter<ChatRoomResponse> responseStream, ServerCallContext context)
    {
        try
        {
            var client = _serverService.FindClient(context.Peer);

            if (!await requestStream.MoveNext())
                throw new ServerException("WARNING: Unexpected End of Stream.");

            if (requestStream.Current.RequestCase is not ChatRoomRequest.RequestOneofCase.Enter)
                throw new ServerException("WARNING: Need EnterRequest.");

            var roomName = requestStream.Current.Enter.RoomName;

            var room = _serverService.FindRoom(roomName);

            using var enter = room.Enter(client, responseStream.SendMessage);
            
            await responseStream.SendEnter(room);
            
            enter.Initialize();

            // Case of only ChatRequest type.
            while (true)
            {
                var result = await requestStream.MoveNext();
                if (!result)
                    break;

                if (requestStream.Current.RequestCase != ChatRoomRequest.RequestOneofCase.Chat)
                {
                    responseStream.SendFail(new ServerException("Unexpected error"));
                    continue;
                }
                
                var chatMessage = requestStream.Current.Chat.Message;

                enter.Broadcast(chatMessage);
            }
        }
        catch (ServerException e)
        {
            responseStream.SendFail(e);
        }
        catch (Exception e)
        {
            // Send fail 넣기.
            Console.WriteLine(e);
        }
    }

    // public override Task<PrevChatsResponse> SendPrevChats(Empty request, ServerCallContext context)
    // {
    //     // context peer 갖고있음
    //     Console.WriteLine("stop");
    //     var room = null;
    //     
    //     var messageList = new RepeatedField<ChatMessageResponse>();
    //
    //     foreach (var message in room.GetPrevChats())
    //     {
    //         messageList.Add(new ChatMessageResponse
    //         {
    //             Message = message.Message,
    //             Nickname = message.Sender.Name,
    //             Time = message.Time.ToTimestamp()
    //         });
    //     }
    //
    //     Console.WriteLine(messageList.Count);
    //
    //     var reply = new PrevChatsResponse
    //     {
    //         PrevChats = { messageList }
    //     };
    //
    //     return Task.FromResult(reply);
    // }
}