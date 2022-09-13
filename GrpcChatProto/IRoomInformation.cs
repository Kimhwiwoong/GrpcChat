namespace GrpcChatProto;

public interface IRoomInformation
{
    string Name { get; }
    
    long ParticipantsCount { get; }
}
