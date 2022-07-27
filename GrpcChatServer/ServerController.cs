using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace GrpcChatServer;

public class ServerController: ChatGrpc.ChatGrpcBase
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
        if (!_serverService.ChangeNick(request.NewName, request.OldName))
        {
            reply.Failed = new FailedResponse
            {
                Reason = "already exist"
            };
            return Task.FromResult(reply);
        }

        reply.Success = new Empty();
        var infoMsg = $"User \"{request.OldName}\" changed nickname to \"{request.NewName}\"";
        Console.WriteLine(infoMsg);

        return Task.FromResult(reply);
    }

    public override Task<SuccessFailResponse> CreateRoom(CreateRoomRequest request, ServerCallContext context)
    {
        var reply = new SuccessFailResponse();
        
        if (!_serverService.CreateRoom(request.Name))
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
        var reply = new ShowRoomsResponse()
        {
            Names = { _serverService.ShowRooms() }
        };

        return Task.FromResult(reply);
    }

    public override async Task EnterRoom(IAsyncStreamReader<ChatRoomRequest> requestStream,
        IServerStreamWriter<ChatRoomResponse> responseStream, ServerCallContext context)
    {
        var currentClient = _serverService.GetCurrentClient(context.Peer);
        currentClient.OnSend += OnCurrentClientOnOnSend;

        // Receive EnterRequest type at first.
        if (!await requestStream.MoveNext())
        {
            Console.WriteLine("WARNING: Unexpected End of Stream.");
            return;
        }
        
        if (requestStream.Current.RequestCase is not ChatRoomRequest.RequestOneofCase.Enter)
        {
            Console.WriteLine("WARNING: Need EnterRequest.");
            return;
        }
        
        var roomName = requestStream.Current.Enter.RoomName;
        var currentRoom = _serverService.GetCurrentRoom(roomName);

        if (currentRoom is null)
        {
            NoRoomResponseSender(responseStream);
            return;
        }

        _serverService.JoinClientToRoom(currentRoom, context.Peer);

        EnteredResponseSender(responseStream);

        // Case of only ChatRequest type.
        while (await requestStream.MoveNext())
        {
            if (requestStream.Current.RequestCase != ChatRoomRequest.RequestOneofCase.Chat)
            {
                Console.WriteLine("WARNING: Need ChatRequest.");
                
                var failedResponse = new ChatRoomResponse
                {
                    Failed = new FailedResponse
                    {
                        Reason = "unexpected error"
                    }
                };
                
                await responseStream.WriteAsync(failedResponse);
                continue;
            }
            
            var chatMessage = 
                $"[{DateTime.Now:yyyy-MM-dd hh:mm:ss tt}] " +
                $"{currentRoom.GetCurrentClientName(context.Peer)} : " +
                $"{requestStream.Current.Chat.Message}";
            currentRoom.Broadcast(context.Peer, chatMessage);
        }
        
        // When client quit.
        currentClient.OnSend -= OnCurrentClientOnOnSend;
        currentRoom.Exit(context.Peer);
        
        void OnCurrentClientOnOnSend(string s) => responseStream.WriteAsync(GetChatResponse(s)).Wait();
    }

    private ChatRoomResponse GetChatResponse(string msg)
    {
        var reply = new ChatRoomResponse
        {
            Chat = new ChatMessage
            {
                Message = msg
            }
        };
        return reply;
    }

    private static async void NoRoomResponseSender(IAsyncStreamWriter<ChatRoomResponse> responseStream)
    {
        await responseStream.WriteAsync(
            new ChatRoomResponse
            {
                Enter = new SuccessFailResponse
                {
                    Failed = new FailedResponse { Reason = "not Found" }
                }
            });
    }

    private static async void EnteredResponseSender(IAsyncStreamWriter<ChatRoomResponse> responseStream)
    {
        await responseStream.WriteAsync(
            new ChatRoomResponse
            {
                Enter = new SuccessFailResponse
                {
                    Success = new Empty()
                }
            }
        );
    }
}