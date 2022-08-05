namespace GrpcChatServer;

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