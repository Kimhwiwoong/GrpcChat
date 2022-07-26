using System.Collections.Concurrent;

namespace GrpcChatServer;

public class ChatRoomManager
{
    private static readonly ConcurrentDictionary<string, ChatRoom> ChatRoomList = new();

    #region Singleton
    private static ChatRoomManager _instance; 
    
    public static ChatRoomManager GetInstance()
    {
        if (_instance is null)
            _instance = new ChatRoomManager();

        return _instance;
    }
    
    private ChatRoomManager()
    {
        
    }
    #endregion
    
    private static bool RoomNameAlready(string target)
    {
        return ChatRoomList.Any(sameName => sameName.Key.Equals(target));
    }

    // public static bool TryCreateRoom(string name, out ChatRoom newRoom)
    // {
    //     newRoom = null;
    //     
    //     if (RoomNameAlready(name))
    //         return false;
    //
    //     newRoom = new ChatRoom(name);
    //     ChatRoomList.TryAdd(name, newRoom);
    //
    //     return true;
    // }
    
    public static bool CreateRoom(string name)
    {
        if (RoomNameAlready(name))
        {
            // Fail -> return false
            return false;
        }
        // Success -> create room and return true
        var newRoom = new ChatRoom(name);
        ChatRoomList.TryAdd(name, newRoom);
        
        return true;
    }
    public static ChatRoom? GetCurrentRoom(string name)
    {
        return ChatRoomList.Values.First(room => room.Name.Equals(name));
    }
}