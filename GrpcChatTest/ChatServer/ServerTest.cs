using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcChatServer;
using NUnit.Framework;

namespace GrpcChatTest.ChatServer;

public class ServerTest
{
    private Channel _channel = null!;
    
    private Server _server = null!;
    private ServerController _controller = null!;
    
    private ChatGrpc.ChatGrpcClient _client = null!;

    [SetUp]
    public void Setup()
    {
        var service = new ServerService();
        _controller = new ServerController(service);
        
        _server = new Server
        {
            Services = { ChatGrpc.BindService(_controller) },
            Ports = { new ServerPort("127.0.0.1", 12345, ServerCredentials.Insecure) }
        };
        
        _server.Start();
        
        _channel = new Channel("127.0.0.1:12345", ChannelCredentials.Insecure);
        _client = new ChatGrpc.ChatGrpcClient(_channel);
    }

    [TearDown]
    public void Teardown()
    {
        _channel.ShutdownAsync().Wait();
        _server.ShutdownAsync().Wait();
    }
    
    [Test]
    public Task TestEnroll()
    {
        _client.Enroll(new Empty());
        
        return Task.CompletedTask;
    }

    [Test]
    public async Task TestChangeNick()
    {
        // 서버에 있는 client목록 이름을 확인하면 되는데 그게 되는지 모르겠음
        // 그전에 클라이언트를 등록하고 그 이름을 바꿔야하는데 그거 안하고 아래부터 해서 에러뜬거
        // test client enroll -> change nickname -> 바꾼 이름을 가진 client(중복 고려) 가져오면 확인 
        // 근데 enroll해도 어떻게 가져오냐 -> enroll부터가 문제네 으하하하하ㅏ하하하하하하하하하하ㅏ하하하ㅏ하하하하하ㅏㅎ하하하하하하하
        var testClient = new Client("1234", "foo");
        var enrollResponse = await _client.EnrollAsync(new Empty());
        var oldNickname = enrollResponse.Nickname;
        
        
        
        var response = await _client.ChangeNickAsync(new ChangeNickRequest()
        {
            // OldName = oldNickname,
            NewName = "bar"
        });

        if (response.Failed is not null)
        {
            Assert.Fail(response.Failed.Reason);
        }
        else
        {
            return;
        }
        // if (response.Failed is { } failed)
        // {
        //     Assert.Fail(failed.Reason);
        //     return;
        // }
        //
        // if (response.Success is { } empty)
        // {
        //     return;
        // }
        //
        // throw new InvalidOperationException();
    }
}