using System.Collections.Concurrent;

namespace GrpcChatServer;

public class ChatRoom
{
    public event Action<string>? OnRemove;
    
    public string Name { get; }
    
    private const int QueueSize = 10;
    private readonly ConcurrentDictionary<string, Client> _clients = new();
    private readonly ConcurrentQueue<MessageContext> _previousChats = new();
    
    private CancellationTokenSource _cts;

    private bool _isWaiting;


    public EnterContext Enter(Client client, Action<MessageContext> action)
    {
        _clients.TryAdd(client.Peer, client);
        Console.WriteLine($"Client {Name} entered room {client.Name}.");
        _cts.Cancel();

        return new EnterContext(client, this, action);
    }

    public void Exit(string peer)
    {
        var currentClient = _clients.Values.First(client => client.Peer.Equals(peer));
        
        _clients.TryRemove(currentClient.Peer, out _);

        if (_isWaiting) return;
        
        _cts = new CancellationTokenSource();
        WaitForRemove(Name, _cts.Token);
    }

    public string GetCurrentClientName(string peer)
    {
        var client = _clients.Values.First(client => client.Peer.Equals(peer));

        return client.Name;
    }

    public int ParticipantsCount()
    {
        return _clients.Count;
    }

    public void Broadcast(string peer, string message)
    {
        if (_previousChats.Count >= QueueSize)
        {
            _previousChats.TryDequeue(out _);
        }

        var context = new MessageContext(_clients[peer], DateTime.Now, message);

        _previousChats.Enqueue(context);

        foreach (var others in _clients.Where(client => client.Key != peer))
        {
            others.Value.Send(context);
        }
    }

    public void SendPrevChat(string peer)
    {
        var currentClient = _clients.Values.First(client => client.Peer.Equals(peer));

        foreach (var chat in _previousChats)
        {
            currentClient.Send(chat);
        }
    }

    private async void WaitForRemove(string chatRoomName, CancellationToken token)
    {
        _isWaiting = true;
        
        try
        {
            await Task.Delay(5000, token);
        }
        catch (OperationCanceledException)
        {
            _isWaiting = false;
            return;
        }
        
        if (ParticipantsCount() == 0) OnRemove!.Invoke(chatRoomName);
        _isWaiting = false;
    }

    public ChatRoom(string chatRoomName)
    {
        Name = chatRoomName;

        if (_isWaiting) return;

        _cts = new CancellationTokenSource();
        WaitForRemove(chatRoomName, _cts.Token);
    }
}