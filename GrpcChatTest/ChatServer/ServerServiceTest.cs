using GrpcChatServer;
using NUnit.Framework;

namespace GrpcChatTest.ChatServer;

public class ServerServiceTest
{
    private ServerService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new ServerService();
    }
    
    [TestCase("foo")]
    [TestCase("bar")]
    public void EnrollTest(string peer)
    {
        _service.Enroll(peer);
        _service.FindClient(peer);
        
        Assert.Pass();
    }

    // [TestCase("aaa")]
    // public void ChangeNickTest(string newName)
    // {
    //     const string peer = "test";
    //     _service.Enroll("test");
    //     
    //     var client = _service.GetCurrentClient(peer);
    //     var oldName = client.Name;
    //
    //     _service.ChangeNick(newName, oldName);
    //
    //     Assert.AreEqual(newName, client.Name);
    // }

    [Test]
    public void CreateRoomTest()
    {
        const string name = "test";
        if (!_service.TryCreateRoom(name))
        {
            Assert.Fail();
        }
        
        Assert.Pass();
    }
}