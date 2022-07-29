using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NUnit.Framework;

namespace GrpcChatTest.ChatClient;

public class MultipleClientTest
{
    private ChatGrpc.ChatGrpcClient _client1 = null!;
    private Channel _channel1 = null!;

    private ChatGrpc.ChatGrpcClient _client2 = null!;
    private Channel _channel2 = null!;

    private Server _server = null!;

    [SetUp]
    public void Setup()
    {
        _server = TestUtility.CreateServer(12345);
        _server.Start();

        (_channel1, _client1) = TestUtility.CreateClient(12345, "client1");
        (_channel2, _client2) = TestUtility.CreateClient(12345, "client2");
    }

    [TearDown]
    public void TearDown()
    {
        Task.WhenAll(
            _channel1.ShutdownAsync(),
            _channel2.ShutdownAsync()
        ).Wait();
        
        
        _server.KillAsync().Wait();
    }

    // [Test]
    // public void MasonTest()
    // {
    //     _client1.Enroll(new Empty());
    //     _client2.Enroll(new Empty());
    //
    //     _client1.CreateRoom(new CreateRoomRequest { Name = "test" });
    //
    //     var call1 = _client1.EnterRoom();
    //     var call2 = _client2.EnterRoom();
    //
    //     call1.RequestStream.WriteAsync(new ChatRoomRequest
    //     {
    //         Enter = new EnterRoomRequest
    //         {
    //             RoomName = "test"
    //         }
    //     });
    //
    //     if (!call1.ResponseStream.MoveNext().Result)
    //     {
    //         Assert.Fail("Enter room of call 1 failed.");
    //     }
    //
    //     call2.RequestStream.WriteAsync(new ChatRoomRequest
    //     {
    //         Enter = new EnterRoomRequest
    //         {
    //             RoomName = "test"
    //         }
    //     });
    //
    //     if (!call2.ResponseStream.MoveNext().Result)
    //     {
    //         Assert.Fail("Enter room of call 2 failed.");
    //     }
    // }

    [Test]
    public async Task TestEnterRoom()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromHours(5));

        var token = cts.Token;

        const int testCount = 5;
        const string testRoom = "world";
        const string testMsg = "hello world!";

        // await Task.WhenAll<EnrollResponse>(
        await _client1.EnrollAsync(new Empty(), cancellationToken: token);
        await _client2.EnrollAsync(new Empty(), cancellationToken: token);
        // );

        await _client1.CreateRoomAsync(new CreateRoomRequest
        {
            Name = testRoom
        });

        // var calls = await Task.WhenAll(
        var call1 = await ChatClientTestUtility.Enter(_client1, testRoom, token);
        var call2 = await ChatClientTestUtility.Enter(_client2, testRoom, token);

        // );

        // var call1 = calls[0];
        // var call2 = calls[1];

        for (var i = 0; i < testCount; i++)
        {
            var message1 = $"{testMsg} ${i} 1";
            var request1 = new ChatRoomRequest
            {
                Chat = new ChatMessageRequest
                {
                    Message = message1
                }
            };

            var message2 = $"{testMsg} ${i} 2";
            var request2 = new ChatRoomRequest
            {
                Chat = new ChatMessageRequest
                {
                    Message = message2
                }
            };

            await Task.WhenAll(
                call1.RequestStream.WriteAsync(request1),
                call2.RequestStream.WriteAsync(request2)
            );

            if (!await call1.ResponseStream.MoveNext(token))
            {
                Assert.Fail();
                return;
            }

            if (!await call2.ResponseStream.MoveNext(token))
            {
                Assert.Fail();
                return;
            }

            // if (call1.ResponseStream.Current.Chat.Message != message2)
            // {
            //     Assert.Fail();
            //     return;
            // }
            //
            // if (call2.ResponseStream.Current.Chat.Message != message1)
            // {
            //     Assert.Fail();
            //     return;
            // }
        }

        Assert.Pass();
    }
}