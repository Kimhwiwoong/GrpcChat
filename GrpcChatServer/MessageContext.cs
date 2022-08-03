namespace GrpcChatServer;

public class MessageContext
{
    public Client Sender { get; }
    public DateTime Time { get; }
    public string Message { get; }

    public MessageContext(Client sender, DateTime time, string message)
    {
        Sender = sender;
        Time = time;
        Message = message;
    }
}