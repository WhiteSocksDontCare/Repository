using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatCommunication
{
    public struct CommandType
    {
        // Server to Client
        // Messages
        public const string Error = "Error";
        public const string Info = "Info";

        // Anwser
        public const string LoginAnswer = "LoginAnswer";
        public const string SubscribeAnswer = "SubscribeAnswer";        
        public const string EditProfileAnswer = "EditProfileAnswer";

        // Update
        public const string UpdateRoom = "UpdateRoom";
        public const string UpdateLobby = "UpdateLobby";

        // Connection
        public const string Login = "Login";        
        public const string Logout = "Logout";
        public const string Subscribe = "Subscribe";
        
        // Profile
        public const string EditProfile = "EditProfile";
        public const string ViewProfile = "ViewProfile";

        // Room
        public const string CreateRoom = "CreateRoom";
        public const string JoinRoom = "JoinRoom";
        public const string LeaveRoom = "LeaveRoom";

        // Message
        public const string SendMessage = "SendMessage";
        public const string DeleteMessage = "DeleteMessage";
        public const string SendLike = "SendLike";
    }

    public struct General
    {
        public const string EOR = "<EOR>";
        public const char CommandDelim = '!';
    }
}
