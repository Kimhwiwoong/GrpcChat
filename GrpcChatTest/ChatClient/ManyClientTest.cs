using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcChatTest.Utility;
using NUnit.Framework;

namespace GrpcChatTest.ChatClient;

public class ManyClientTest
{
    private const int TestCount = 5;

    private Server _server = null!;
    private Dictionary<ChatGrpc.ChatGrpcClient, Channel> _clientChannels = null!;

    private Dictionary<ChatGrpc.ChatGrpcClient, AsyncDuplexStreamingCall<ChatRoomRequest, ChatRoomResponse>> _calls =
        null!;


    [SetUp]
    public void SetUp()
    {
        _server = TestUtility.CreateServer(12345);
        _server.Start();

        _clientChannels = TestUtility.MakeManyClient(TestCount);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var channel in _clientChannels.Values)
        {
            channel.ShutdownAsync().Wait();
        }

        foreach (var call in _calls.Values)
        {
            call.Dispose();
        }

        _server.ShutdownAsync().Wait();
    }

    [Test]
    // [Timeout(5000)]
    [Repeat(1)]
    public async Task TestEnterRoom()
    {
        const string testRoom = "world";
        
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        _calls = new Dictionary<ChatGrpc.ChatGrpcClient, AsyncDuplexStreamingCall<ChatRoomRequest, ChatRoomResponse>>();

        foreach (var client in _clientChannels.Keys)
        {
            await client.EnrollAsync(new Empty(), cancellationToken: token);
        }

        await _clientChannels.ElementAt(0).Key.CreateRoomAsync(new CreateRoomRequest
        {
            Name = testRoom
        });


        foreach (var client in _clientChannels.Keys)
        {
            var call = await ChatClientTestUtility.Enter(client, testRoom, token);
            _calls.Add(client, call);
        }

        // );
        foreach (var client in _clientChannels.Keys)
        {
            var message = $"testMessage {client}";

            await _calls[client].RequestStream.WriteAsync(new ChatRoomRequest()
            {
                Chat = new ChatMessageRequest
                {
                    Message = message
                }
            });
        }

        foreach (var call in _calls.Values)
        {
            for (var i = 0; i < 2 * (TestCount - 1); i++)
            {
                if (!await call.ResponseStream.MoveNext(token))
                {
                    Assert.Fail();
                }

                var response = call.ResponseStream.Current;

                Console.WriteLine(response.ResponseCase.ToString());
                Console.WriteLine(response.Chat?.Message);
            }
        }
        
        // 예외 일으키면 성공
        var restReadTasks = _calls.Values.Select(async call =>
        {
            if (!await call.ResponseStream.MoveNext(token))
                return null; 

            return call.ResponseStream.Current;
        }).ToArray();

        try
        {
            var any = await Task.WhenAny(restReadTasks);
            var response = await any;

            Console.WriteLine("Remain:");
            Console.WriteLine(response?.ResponseCase.ToString());
            Console.WriteLine(response?.Chat?.Message);
            Assert.Fail("Some messages remained");
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
        {
            Assert.Pass();
        }
    }
}