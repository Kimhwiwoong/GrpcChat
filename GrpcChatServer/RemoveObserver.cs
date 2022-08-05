namespace GrpcChatServer;

public interface IRemoveEventHandler
{
    event Action? OnRemove;
}
public class RemoveObserver : IObserver<int>, IRemoveEventHandler
{
    public event Action? OnRemove;

    private CancellationTokenSource _cts = new();
    private bool _isWaiting;

    public RemoveObserver()
    {
        _isWaiting = false;
    }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnNext(int duration)
    {
        if (_isWaiting) return;

        _cts = new CancellationTokenSource();
        WaitForRemove(duration, _cts.Token);
    }

    public void Cancel()
    {
        if (!_isWaiting)
        {
            return;
        }
        
        _cts.Cancel();
    }
    
    private async void WaitForRemove(int duration, CancellationToken token)
    {
        _isWaiting = true;
        
        try
        {
            await Task.Delay(duration, token);
        }
        catch (OperationCanceledException)
        {
            _isWaiting = false;
            return;
        }
        
        OnRemove?.Invoke();
        _isWaiting = false;
    }
}