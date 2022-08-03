namespace GrpcChat.Exceptions;

public class EnterRoomException : Exception
{
    public EnterRoomException(string reason) : base(reason)
    {
    }
}