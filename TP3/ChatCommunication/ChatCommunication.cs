using System;
using System.Collections.Generic;
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
        Profile ClientProfile { get; set; }
        List<Profile> OtherUsers { get; set; }
        List<Room> AllRooms { get; set; }
    }

    public class Profile
    {
        string Pseudo { get; set; }
        string LastName { get; set; }
        string FirstName { get; set; }
        int IDRoom { get; set; }
        int NbLike { get; set; }
        int NbDislike { get; set; }
        int NbMessage { get; set; }
        int NbDeletedMessage { get; set; }
        bool IsConnected { get; set; }

        [XmlIgnore]
        public BitmapSource Avatar { get; set; }

        [XmlElement("Avatar")]
        public byte[] ImageBuffer
        {
            get
            {
                byte[] imageBuffer = null;

                if (Avatar != null)
                {
                    using (var stream = new MemoryStream())
                    {
                        var encoder = new PngBitmapEncoder(); // or some other encoder
                        encoder.Frames.Add(BitmapFrame.Create(Avatar));
                        encoder.Save(stream);
                        imageBuffer = stream.ToArray();
                    }
                }

                return imageBuffer;
            }
            set
            {
                if (value == null)
                {
                    Avatar = null;
                }
                else
                {
                    using (var stream = new MemoryStream(value))
                    {
                        var decoder = BitmapDecoder.Create(stream,
                            BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                        Avatar = decoder.Frames[0];
                    }
                }
            }
        }
    }

    public class Room
    {
        int IDRoom { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        bool IsDeleted { get; set; }
        List<Profile> SubscribedUsers { get; set; }
        List<Message> Messages { get; set; }
    }

    public class User
    {
        string Pseudo { get; set; }
        string Password { get; set; }
    }

    public class Message
    {
        int IDMessage { get; set; }
        string Text { get; set; }
        bool IsDeleted{ get; set; }
        string Pseudo { get; set; }
        int IDRoom { get; set; }
        int NbLike { get; set; }
        int NbDislike { get; set; }
    }

    public class Like
    {
        int IDMessage { get; set; }
        string Pseudo { get; set; }
        bool IsLike { get; set; }
    }

}
