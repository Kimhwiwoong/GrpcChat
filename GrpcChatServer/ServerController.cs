using System.Collections.Concurrent;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace GrpcChatServer;

public class ServerController: ChatGrpc.ChatGrpcBase
{
    private readonly ServerService _serverService = new();
    private readonly ConcurrentDictionary<string, List<IServerStreamWriter<RoomResponse>>> _streamList = new();

    public override Task<EnrollClient> Enroll(Empty request, ServerCallContext context)
    {
        var nickname = _serverService.Enroll(context.Peer);
        var reply = new EnrollClient
        {
            Nickname = nickname
        };

        return Task.FromResult(reply);
    }

    public override Task<ChangeNickResponse> ChangeNick(Nickname request, ServerCallContext context)
    {
        var reply = new ChangeNickResponse();
        if (!_serverService.ChangeNick(request.NewName, request.OldName))
        {
            reply.Failed = new FailedResponse
            {
                Reason = "already exist"
            };
            return Task.FromResult(reply);
        }

        reply.Success = request;
        var infoMsg = $"User \"{request.OldName}\" changed nickname to \"{request.NewName}\"";
        Console.WriteLine(infoMsg);

        return Task.FromResult(reply);
    }

    public override Task<CreateResponse> CreateRoom(RoomInformation request, ServerCallContext context)
    {
        var reply = new CreateResponse();
        
        if (!_serverService.CreateRoom(request.Name))
        {
            reply.Failed = new FailedResponse
            {
                Reason = "Room name already exist"
            };
            return Task.FromResult(reply);
        }
        
        _streamList.TryAdd(request.Name, new List<IServerStreamWriter<RoomResponse>>());
        
        reply.Success = new Empty();

        return Task.FromResult(reply);
    }

    public override Task<Rooms> ShowRooms(Empty request, ServerCallContext context)
    {
        var reply = new Rooms()
        {
            Names = { _serverService.ShowRooms() }
        };

        return Task.FromResult(reply);
    }

    public override async Task EnterRoom(IAsyncStreamReader<RoomRequest> requestStream,
        IServerStreamWriter<RoomResponse> responseStream, ServerCallContext context)
    {
        var currentClient = _serverService.GetCurrentClient(context.Peer);
        currentClient.OnSend += OnCurrentClientOnOnSend;

        // Receive EnterRequest type at first.
        if (!await requestStream.MoveNext())
        {
            Console.WriteLine("WARNING: Unexpected End of Stream.");
            return;
        }
        
        if (requestStream.Current.RequestCase is not RoomRequest.RequestOneofCase.Enter)
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

        _serverService.JoinClientToRoom(currentRoom!, context.Peer);

        EnteredResponseSender(currentRoom!, responseStream);

        // Case of only ChatRequest type.
        while (await requestStream.MoveNext())
        {
            if (requestStream.Current.RequestCase != RoomRequest.RequestOneofCase.Chat)
            {
                Console.WriteLine("WARNING: Need ChatRequest.");
                
                var failedResponse = new RoomResponse
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
                $"{currentRoom!.GetCurrentClientName(context.Peer)} : " +
                $"{requestStream.Current.Chat.Message}";
            currentRoom.Broadcast(context.Peer, chatMessage);
        }
        
        // When client quit.
        currentClient.OnSend -= OnCurrentClientOnOnSend;
        currentRoom?.Exit(context.Peer);
        
        void OnCurrentClientOnOnSend(string s) => responseStream.WriteAsync(GetChatResponse(s)).Wait();
    }

    private RoomResponse GetChatResponse(string msg)
    {
        var reply = new RoomResponse
        {
            Chat = new ChatMessage
            {
                Message = msg
            }
        };
        return reply;
    }

    private static async void NoRoomResponseSender(IAsyncStreamWriter<RoomResponse> responseStream)
    {
        await responseStream.WriteAsync(
            new RoomResponse
            {
                Enter = new EnterResponse
                {
                    Failed = new FailedResponse { Reason = "not Found" }
                }
            });
    }

    private static async void EnteredResponseSender(ChatRoom room, IAsyncStreamWriter<RoomResponse> responseStream)
    {
        await responseStream.WriteAsync(
            new RoomResponse
            {
                Enter = new EnterResponse
                {
                    Success = new RoomInformation
                    {
                        Name = room.Name
                    }
                }
            }
        );
    }
}