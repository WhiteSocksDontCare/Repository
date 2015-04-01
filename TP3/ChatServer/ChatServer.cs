using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using ChatCommunication;
using System.Timers;

namespace ChatServer
{
    /// <summary>
    /// http://msdn.microsoft.com/en-us/library/fx6588te(v=vs.110).aspx
    /// http://fr.slideshare.net/ZGTRZGTR/c-sharp-advanced-l08-networkingwcf
    /// </summary>

    public class StateObject
    {
        // Client  socket.
        public Socket WorkSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] Buffer = new byte[BufferSize];
    }

    class ChatServer
    {
        private static double SAVE_INTERVAL = 600000;  //Intervale de temps entre deux save des listes dans le fichier XML (10 minutes)
        private static int i = 0;
        public static ManualResetEvent AllDone = new ManualResetEvent(false);
        private static Dictionary<string, Socket> clients = new Dictionary<string, Socket>();

        public static void LoadServerInfos()
        {
            //Test temporaire pour vérifier le fonctionnement des fichiers XML
            try
            {
                ChatCommunication.User test = ChatCommunication.SerializerHelper.DeserializeFromXML<ChatCommunication.User>("test.xml");
                //Console.Out.WriteLine("Deserialized : Pseudo(" + test.Pseudo + ") Password(" + test.Password + ")");
            }
            catch(Exception ex)
            {
                Console.Out.WriteLine(ex.InnerException.Message);
            }
        }
        public static void ServerInfosTimer()
        {
            System.Timers.Timer saveTimer = new System.Timers.Timer(SAVE_INTERVAL);

            saveTimer.AutoReset = true;
            saveTimer.Elapsed += new ElapsedEventHandler(SaveServerInfos);

            saveTimer.Start();
        }

        public static void SaveServerInfos(object source, ElapsedEventArgs e)
        {
            //Test temporaire pour vérifier le fonctionnement des fichiers XML
            try
            {
                ChatCommunication.User test = new ChatCommunication.User();
                DateTime date = DateTime.Now;
                test.Pseudo = date.ToLongDateString();
                test.Password = date.ToLongTimeString();
                //Console.Out.WriteLine("Serialized : Pseudo(" + test.Pseudo + ") Password(" + test.Password + ")");

                ChatCommunication.SerializerHelper.SerializeToXML(test, "test.xml");
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.InnerException.Message);
            }
        }

        private static Dictionary<Socket, Profile> onlineClients = new Dictionary<Socket, Profile>();
        private static List<Profile> profiles = new List<Profile>();
        private static List<Room> rooms = new List<Room>();
        private static List<Like> likes = new List<Like>();
        private static List<User> users = new List<User>();

        public static void StartListening()
        {
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList[1];
            var localEndPoint = new IPEndPoint(ipAddress, 11000);

            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    AllDone.Reset();

                    Console.WriteLine("Waiting for someone...");
                    listener.BeginAccept(AcceptCallback, listener);
                    AllDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress enter to continue");
            Console.ReadLine();
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            AllDone.Set();

            var listener = (Socket)ar.AsyncState;
            var handler = listener.EndAccept(ar);

            var state = new StateObject { WorkSocket = handler };

            handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                ReadCallback, state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            var state = (StateObject)ar.AsyncState;
            var handler = state.WorkSocket;

            // Read data from the client socket. 
            var bytesRead = handler.EndReceive(ar);

            if (bytesRead <= 0) return;

            var data = Encoding.ASCII.GetString(state.Buffer, 0, bytesRead);

            var messageArray = data.Split(new[] { '!' }, 2);
            var commandType = messageArray[0];

            switch (commandType)
            {
                case "Login":
                    {
                        TryConnect(handler, messageArray[1].Deserialize<User>());
                        break;
                    }
                case "Subscribe":
                    {
                        Subscribe(handler, messageArray[1].Deserialize<User>());
                        break;
                    }
                case "Logout":
                    {
                        Logout(handler);
                        break;
                    }

                case "CreateProfile":
                {
                    CreateProfile(messageArray[1].Deserialize<Profile>());
                        break;
                    }
                case "EditProfile":
                {
                    EditProfile(handler, messageArray[1].Deserialize<Profile>());
                        break;
                    }
                case "ViewProfile":
                {
                    ViewProfile(handler, messageArray[1]);
                        
                        break;
                    }
                case "CreateRoom":
                {
                    CreateRoom(handler, messageArray[1].Deserialize<Room>());
                        
                        break;
                    }
                case "JoinRoom":
                {
                    JoinRoom(handler, Convert.ToInt32(messageArray[1]));
                        break;
                    }
                case "LeaveRoom":
                {
                    LeaveRoom(handler);
                        break;
                    }
                case "SendMessage":
                    {
                        SendMessage(messageArray[1].Deserialize<Message>());
                        break;
                    }
                case "DeleteMessage":
                    {
                        DeleteMessage(messageArray[1].Deserialize<Message>());
                        break;
                    }
                case "SendLike":
                {
                    SendLike(messageArray[1].Deserialize<Like>());
                        break;
                    }
                default:
                    throw new Exception();
            }
            Console.WriteLine(commandType);
        }

        public static void Send(Socket handler, string commandType, string data)
        {
            data = commandType + "!" + data;
            var byteData = Encoding.ASCII.GetBytes(data);

            handler.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, handler);
        }

        public static void SendCallback(IAsyncResult ar)
        {
            try
            {
                var handler = (Socket)ar.AsyncState;

                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Verifie les credentials dans la liste des user, si trouve on ajoute le profile aux onlineClients,
        /// Sinon on retourne un message d'erreur
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="user"></param>
        private static void TryConnect(Socket handler, User user)
        {
            if (users.Find(x => x.Pseudo == user.Pseudo && x.Password == user.Password) == null)
            {
                Send(handler, "Error", "Nom d'usager ou mot de passe invalide");
                return;
            }

            var profile = profiles.Find(x => x.Pseudo == user.Pseudo);
            onlineClients[handler] = profile;

        }

        /// <summary>
        /// Créer un profile vide en remplissant juste le Pseudo, ajoute le user à la liste et retourne le profil
        /// au client pour qu'il puisse le compléter
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="user"></param>
        private static void Subscribe(Socket handler, User user)
        {
            var bidon = new Profile { Pseudo = user.Pseudo };
            onlineClients[handler] = bidon;
            users.Add(user);
            Send(handler, "Subscribe", bidon.Serialize());
        }

        /// <summary>
        /// Enleve le tuple du dictionaire en fonction du socket
        /// </summary>
        /// <param name="handler"></param>
        private static void Logout(Socket handler)
        {
            onlineClients.Remove(handler);
        }

        /// <summary>
        /// Ajoute le profile passé en paramètre à la liste profiles
        /// </summary>
        /// <param name="profile"></param>
        private static void CreateProfile(Profile profile)
        {
            profiles.Add(profile);
        }

        /// <summary>
        /// Trouve le profil correspondant au nom du nouveau profil dans la liste de profil
        /// et affecte le profil modifié
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="newProfile"></param>
        private static void EditProfile(Socket socket, Profile newProfile)
        {
            var profile = profiles.Find(x => x.Pseudo == newProfile.Pseudo);
            profiles[profiles.IndexOf(profile)] = newProfile;
            Send(socket, "Info", "Le profile a été mis à jour");
        }

        /// <summary>
        /// Trouve le profile correspondant au nom d'usager et le retourne au client
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="username"></param>
        private static void ViewProfile(Socket socket, string username)
        {
            var profile = profiles.Find(x => x.Pseudo == username);
            var serializedProfile = profile.Serialize();
            Send(socket, "ViewProfile", serializedProfile);
        }

        /// <summary>
        /// Ajoute la salle à la liste et retourne la salle
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="room"></param>
        private static void CreateRoom(Socket handler, Room room)
        {
            rooms.Add(room);
            Send(handler, "UpdateRoom", room.Serialize());
        }

        /// <summary>
        /// Met à jour le id de la salle du profil de l'utilisateur
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="idRoom"></param>
        private static void JoinRoom(Socket socket, int idRoom)
        {
            onlineClients[socket].IDRoom = idRoom;
            var room = rooms.Find(x => x.IDRoom == idRoom);
            room.SubscribedUsers.Add(onlineClients[socket]);
            Send(socket, "UpdateRoom", room.Serialize());
        }

        /// <summary>
        /// Met à jour le id de la salle du profil à -1
        /// </summary>
        /// <param name="handler"></param>
        private static void LeaveRoom(Socket handler)
        {
            onlineClients[handler].IDRoom = -1;
            //Send();
        }

        /// <summary>
        /// Trouve les profils reliés au id de la salle de discussion. Trouve ensuite les socket reliés à chaque profil
        /// dans le dictionnaire onlineClients. Envoie finalement la salle à tous les sockets trouvés.
        /// </summary>
        /// <param name="message"></param>
        public static void SendMessage(Message message)
        {
            var room = rooms.Find(x => x.IDRoom == message.IDRoom);
            List<Socket> listSockets = onlineClients.Where(x => room.SubscribedUsers.All(p => p == x.Value)).Select(x => x.Key).ToList();

            room.Messages.Add(message);

            foreach (var socket in listSockets)
            {
                Send(socket, "UpdateRoom", room.Serialize());
            }
        }

        /// <summary>
        /// Trouve les profils reliés au id de la salle de discussion. Trouve ensuite les socket reliés à chaque profil
        /// dans le dictionnaire onlineClients. Envoie finalement la salle à tous les sockets trouvés.
        /// </summary>
        /// <param name="message"></param>
        public static void DeleteMessage(Message message)
        {
            var room = rooms.Find(x => x.IDRoom == message.IDRoom);
            List<Socket> listSockets = onlineClients.Where(x => room.SubscribedUsers.All(p => p == x.Value)).Select(x => x.Key).ToList();

            room.Messages.Remove(message);

            foreach (var socket in listSockets)
            {
                Send(socket, "UpdateRoom", room.Serialize());
            }
        }

        private static void SendLike(Like like)
        {
            likes.Add(like);
        }

    }
}
