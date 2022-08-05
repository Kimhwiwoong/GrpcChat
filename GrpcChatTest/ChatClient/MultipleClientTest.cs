using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcChatTest.Utility;
using NUnit.Framework;

namespace GrpcChatTest.ChatClient;

[Timeout(10000)]
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
        Console.WriteLine(1);
        
        _server = TestUtility.CreateServer(12345);
        
        Console.WriteLine(2);
        
        _server.Start();
        
        Console.WriteLine(3);

        (_channel1, _client1) = TestUtility.CreateClient(12345, "client1");
        
        Console.WriteLine(4);
        
        (_channel2, _client2) = TestUtility.CreateClient(12345, "client2");

        Console.WriteLine("SETUP COMPLETE");
    }

    [TearDown]
    public void TearDown()
    {
        Console.WriteLine("TEARDOWN");
        
        Task.WhenAll(
            _channel1.ShutdownAsync(),
            _channel2.ShutdownAsync()
        ).Wait();

        _server.ShutdownAsync().Wait();

        Console.WriteLine("TEARDOWN COMPLETE");
    }

    [Test]
    [Repeat(80)]
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
        using var call1 = await ChatClientTestUtility.Enter(_client1, testRoom, token);
        using var call2 = await ChatClientTestUtility.Enter(_client2, testRoom, token);

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