namespace GrpcChatServer;

// NOTE: 이름을 헷갈리지 않게 리팩토링.
public class MessageData
{
    public Client Sender { get; }
    public DateTime Time { get; }
    public string Message { get; }

    public MessageData(Client sender, DateTime time, string message)
    {
        Sender = sender;
        Time = time;
        Message = message;
    }
}