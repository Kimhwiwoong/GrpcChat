namespace GrpcChatServer;

public interface IKillEventHandler
{
    event Action? OnKilled;
}

public class KillObserver : IObserver<int>, IKillEventHandler
{
    public event Action? OnKilled;

    private CancellationTokenSource? _cts;
    private bool _isWaiting;

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnNext(int milliseconds)
    {
        if (_isWaiting)
            return;

        _cts = new CancellationTokenSource();
        WaitForRemove(milliseconds, _cts.Token);
    }

    public void Cancel() => _cts?.Cancel();

    private async void WaitForRemove(int milliseconds, CancellationToken token)
    {
        _isWaiting = true;
        
        try
        {
            await Task.Delay(milliseconds, token);
        }
        catch (OperationCanceledException)
        {
            _isWaiting = false;
            return;
        }
        
        // Invoke.
        OnKilled?.Invoke();
        _isWaiting = false;
    }
}