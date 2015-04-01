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
        public Profile ClientProfile { get; set; }
        public List<Profile> OtherUsers { get; set; }
        public List<Room> AllRooms { get; set; }
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
        public int IDRoom { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public List<Profile> SubscribedUsers { get; set; }
        public List<Message> Messages { get; set; }
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
