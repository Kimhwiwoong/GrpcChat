namespace GrpcChatServer.Exceptions;

public class ServerException : Exception
{
    public ServerException(string message) : base(message)
    {
    }

    public ServerException(string message, Exception innerException) : base(message, innerException)
    {
    }
}