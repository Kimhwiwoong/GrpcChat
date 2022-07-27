using GrpcChatServer;
using NUnit.Framework;

namespace GrpcChatTest.ChatServer;

public class ServerServiceTest
{
    private ServerService _service;

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
        _service.GetCurrentClient(peer);
        
        Assert.Pass();
    }

    [TestCase("aaa")]
    public void ChangeNickTest(string newName)
    {
        const string peer = "test";
        _service.Enroll("test");
        
        var client = _service.GetCurrentClient(peer);
        var oldName = client.Name;

        _service.ChangeNick(newName, oldName);

        Assert.AreEqual(newName, client.Name);
    }

    [TestCase("testRoom")]
    public void CreateRoomTest(string roomName)
    {
        const string name = "test";
        var success = "yes";
        
        _service.CreateRoom(name);
        
        if (_service.GetCurrentRoom(name) == null)
        {
            success = "no";
        }

        Assert.AreEqual("yes",success);
        
    }
}