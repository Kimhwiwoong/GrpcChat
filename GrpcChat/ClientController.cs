using GrpcChat.Exceptions;

namespace GrpcChat;

public class ClientController
{
    private readonly ClientService _clientService;

    public ClientController(ClientService clientService)
    {
        _clientService = clientService;
    }
    
    public void Start()
    {
        Enroll();
        
        while (true)
        {
            try
            {
                switch (Console.ReadLine())
                {
                    case "create_room" :
                        CreateRoom();
                        break;
                        
                    case "show_rooms" :
                        ShowRooms();
                        break;
                        
                    case "change_nickname" :
                        ChangeNickname();
                        break;
                        
                    case "enter_room" : 
                        EnterRoom();
                        break;
                        
                    case "exit":
                        Console.WriteLine("GoodBye");
                        continue;

                    default:
                        Console.WriteLine("Wrong command");
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                break;
            }
        }
    }

    private void Enroll()
    {
        _clientService.Enroll();
            
        Console.WriteLine("______________________________________________");
        Console.WriteLine("change_nickname   :   닉변                     ");
        Console.WriteLine("create_room       :   방 생성                   ");
        Console.WriteLine("show_rooms        :   방 목록 조회               ");
        Console.WriteLine("enter_room        :   방 입장                   ");
        Console.WriteLine("exit              :   종료                     ");
        Console.WriteLine("______________________________________________");
        Console.WriteLine($"Your Nickname : {_clientService.GetCurrentNickname()}");
    }
    
    private void CreateRoom()
    {
        while (true)
        {
            try
            {
                Console.Write("Enter room name to create (or enter /quit to exit) : ");
                var roomName = Console.ReadLine();

                if (!Validate(roomName!))
                    break;

                _clientService.CreateRoom(roomName!);
                Console.WriteLine("Room Created.");
                return;
            }
            catch (InvalidNameException)
            {
                Console.WriteLine("Invalid name.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    private static bool Validate(string name)
    {
        if(string.IsNullOrWhiteSpace(name))
            throw new InvalidNameException();
 
        return name != "/quit";
    }

    private void ChangeNickname()
    {
        while (true)
        {
            try
            {
                Console.WriteLine("Enter new nickname to use : ");
                var newNickname = Console.ReadLine();
                
                if(string.IsNullOrWhiteSpace(newNickname))
                    continue;

                if (newNickname == "/quit")
                    break;
                
                _clientService.ChangeNickname(newNickname);
                Console.WriteLine($"Your new Nickname : {newNickname}");
                break;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
    
    private void ShowRooms()
    {
        var roomsInfo = _clientService.ShowRooms();
        foreach (var room in roomsInfo)
        {
            Console.WriteLine(room);
        }
    }

    private void EnterRoom()
    {
        Console.WriteLine("room name to enter : ");
        var roomName = Console.ReadLine();

        try
        {
            var room = _clientService.EnterRoom(roomName!);
            room.OnMessage += (message) =>
            {
                Console.WriteLine(message);
            };

            while (true)
            {
                var line = Console.ReadLine();
                
                // Do not any message to server -> server can't know user quit chatting room.
                if (line == "quit")
                    break;
                
                room.SendMessage(line!);
            }
            
            room.Exit();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}