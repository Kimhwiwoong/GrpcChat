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
        
        //_serverService.CheckRemoveRoom(request.Name);
        
        return Task.FromResult(reply);
    }

    public override Task<ShowRoomsResponse> ShowRooms(Empty request, ServerCallContext context)
    {
        var infoList = new RepeatedField<RoomInformationResponse>();

        foreach (var room in _serverService.ChatRooms.Values)
        {
            infoList.Add(new RoomInformationResponse
            {
                Name = room.Name,
                ParticipantsCount = room.ParticipantsCount()
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
            if (!_serverService.Clients.TryGetValue(context.Peer, out var client))
                throw new ServerException("No such client exist.");

            if (!await requestStream.MoveNext())
                throw new ServerException("WARNING: Unexpected End of Stream.");

            if (requestStream.Current.RequestCase is not ChatRoomRequest.RequestOneofCase.Enter)
                throw new ServerException("WARNING: Need EnterRequest.");

            var roomName = requestStream.Current.Enter.RoomName;
            
            if(!_serverService.ChatRooms.TryGetValue(roomName, out var room))
                throw new ServerException("No such room exist.");

            using var enter = room.Enter(client, responseStream.SendMessage);
            responseStream.SendEnter();

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
    }
}