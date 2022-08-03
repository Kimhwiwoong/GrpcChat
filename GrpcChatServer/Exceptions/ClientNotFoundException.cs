namespace GrpcChatServer.Exceptions;

public class ClientNotFoundException : ServerException
{
    public ClientNotFoundException(string peer) : base($"Client not found: {peer}") 
    {
    }
}