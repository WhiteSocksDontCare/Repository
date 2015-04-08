using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace ChatCommunication
{
    public class Lobby
    {
        public Profile ClientProfile { get; set; }
        public ObservableCollection<Profile> OtherUsers { get; set; }
        public ObservableCollection<Room> AllRooms { get; set; }

        public Lobby()
        {
            OtherUsers = new ObservableCollection<Profile>();
            AllRooms = new ObservableCollection<Room>();
        }
    }

    public class Profile
    {
        public string Pseudo { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public int IDRoom { get; set; }
        public int NbLike { get; set; }
        public int NbDislike { get; set; }
        public int NbMessage { get; set; }
        public int NbDeletedMessage { get; set; }
        public bool IsConnected { get; set; }
        public string AvatarUri { get; set; }
    }

    public class Room
    {
        public int IDRoom { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public ObservableCollection<Profile> SubscribedUsers { get; set; }
        public ObservableCollection<Message> Messages { get; set; }

        public Room()
        {
            SubscribedUsers = new ObservableCollection<Profile>();
            Messages = new ObservableCollection<Message>();
        }
    }

    public class User
    {
        public string Pseudo { get; set; }
        public string Password { get; set; }
    }

    public class Message
    {
        public int IDMessage { get; set; }
        public string Text { get; set; }
        public bool IsDeleted { get; set; }
        public string Pseudo { get; set; }
        public int IDRoom { get; set; }
        public int NbLike { get; set; }
        public int NbDislike { get; set; }
    }

    public class Like
    {
        public int IDMessage { get; set; }
        public string Pseudo { get; set; }
        public bool IsLike { get; set; }
    }

}
