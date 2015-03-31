using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChatCommunication
{
    public class Lobby
    {

    }
    public class Profile
    {
        string Pseudo;
        string LastName;
        string FirstName;
        byte[] Avatar;
        int IDRoom;
        int NbLike;
        int NbDislike;
        int NbMessage;
        int NbDeletedMessage;
        bool IsConnected;
    }
    public class Room
    {
        int IDRoom;
        string Name;
        string Description;
        bool IsDeleted;
        List<Profile> SubscribedUsers;
        List<Message> Messages;
    }
    public class User
    {
        string Pseudo;
        string Password;
    }
    public class Message
    {
        int IDMessage;
        string Text;
        bool IsDeleted;
        string Pseudo;
        int IDRoom;
        int NbLike;
        int NbDislike;
    }
    public class Like
    {
        int IDMessage;
        string Pseudo;
        bool IsLike;
    }

}
