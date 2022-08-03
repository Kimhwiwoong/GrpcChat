namespace GrpcChatServer.Exceptions;

public class RoomNotFoundException : ServerException
{
    public RoomNotFoundException(string roomName) : base($"Room not found: {roomName}")
    {
    }
}