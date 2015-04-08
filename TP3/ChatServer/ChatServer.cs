using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using ChatCommunication;
using System.Timers;
using System.Collections.ObjectModel;

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
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    class ChatServer //: IDisposable
    {
        //Jajoute un 0 pour pas se faire spammer en debug
        private static double UPDATE_INTERVAL = 100000;  //Intervale de temps entre deux mises à jour des lobbys vers les clients (30 secondes)
        private static double SAVE_INTERVAL = 600000;  //Intervale de temps entre deux save des listes dans le fichier XML (10 minutes)
        private static string PROFILES_FILE = "profiles.xml";
        private static string ROOMS_FILE = "rooms.xml";
        private static string LIKES_FILE = "likes.xml";
        private static string USERS_FILE = "users.xml";

        private static Semaphore semaphoreLobby = new Semaphore(1, 1);
        private static Socket _listener;
        public static ManualResetEvent AllDone = new ManualResetEvent(false);
        private static Dictionary<Socket, Profile> onlineClients = new Dictionary<Socket, Profile>();

        private static List<Profile> profiles = new List<Profile>();
        private static List<Room> rooms = new List<Room>();
        private static List<Like> likes = new List<Like>();
        private static List<User> users = new List<User>();
        private static List<Message> messages = new List<Message>();
        private static Lobby lobby = new Lobby();
        private bool disposed = true;

        public static void CleanUp()
        {
            try
            {
                _listener.Close();
                _listener = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            foreach (var client in onlineClients)
            {
                client.Key.Shutdown(SocketShutdown.Both);
            client.Key.Disconnect(false);
                client.Key.Close();
            }

            ChatCommunication.SerializerHelper.SerializeToXML(profiles, PROFILES_FILE);
            ChatCommunication.SerializerHelper.SerializeToXML(rooms, ROOMS_FILE);
            ChatCommunication.SerializerHelper.SerializeToXML(likes, LIKES_FILE);
            ChatCommunication.SerializerHelper.SerializeToXML(users, USERS_FILE);
        }

        public static void LoadServerInfos()
        {
            List<Profile> tempProfiles;
            if ((tempProfiles = ChatCommunication.SerializerHelper.DeserializeFromXML<List<Profile>>(PROFILES_FILE)) != null)
                profiles = tempProfiles;

            List<Room> tempRooms;
            if ((tempRooms = ChatCommunication.SerializerHelper.DeserializeFromXML<List<Room>>(ROOMS_FILE)) != null)
                rooms = tempRooms;

            List<Like> tempLikes;
            if ((tempLikes = ChatCommunication.SerializerHelper.DeserializeFromXML<List<Like>>(LIKES_FILE)) != null)
                likes = tempLikes;

            List<User> tempUsers;
            if ((tempUsers = ChatCommunication.SerializerHelper.DeserializeFromXML<List<User>>(USERS_FILE)) != null)
                users = tempUsers;
        }

        public static void ServerInfosTimer()
        {
            System.Timers.Timer saveTimer = new System.Timers.Timer(SAVE_INTERVAL);

            saveTimer.AutoReset = true;
            saveTimer.Elapsed += new ElapsedEventHandler(ServerInfosTimerElapsed);

            saveTimer.Start();
        }

        public static void ServerInfosTimerElapsed(object source, ElapsedEventArgs e)
        {
            //TODO : Décider si un sémaphore nécessaire (dangereux de sérializer si autre thread ajoute dans une liste?)
            ChatCommunication.SerializerHelper.SerializeToXML(profiles, PROFILES_FILE);
            ChatCommunication.SerializerHelper.SerializeToXML(rooms, ROOMS_FILE);
            ChatCommunication.SerializerHelper.SerializeToXML(likes, LIKES_FILE);
            ChatCommunication.SerializerHelper.SerializeToXML(users, USERS_FILE);
        }

        public static void UpdateLobbyTimer()
        {
            System.Timers.Timer updateTimer = new System.Timers.Timer(UPDATE_INTERVAL);

            updateTimer.AutoReset = true;
            updateTimer.Elapsed += new ElapsedEventHandler(UpdateLobbyTimerElapsed);

            updateTimer.Start();
        }

        public static void UpdateLobbyTimerElapsed(object source, ElapsedEventArgs e)
        {
            foreach (KeyValuePair<Socket, Profile> client in onlineClients)
                UpdateLobby(client.Key, client.Value);
        }

        public static void StartListening()
        {
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = null;
            foreach (var addr in Dns.GetHostEntry(string.Empty).AddressList.Where(addr => addr.AddressFamily == AddressFamily.InterNetwork))
            {
                ipAddress = addr;
            }
            var localEndPoint = new IPEndPoint(ipAddress, 11000);

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                _listener.Bind(localEndPoint);
                _listener.Listen(100);

                while (true)
                {
                    AllDone.Reset();

                    Console.WriteLine("Waiting for someone...");
                    if (_listener == null)
                        return;
                    _listener.BeginAccept(AcceptCallback, _listener);
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
            if (_listener == null)
                return;
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

            if (!handler.Connected)
                return;

            // Read data from the client socket. 
            var bytesRead = handler.EndReceive(ar);

            // There might be more data, so store the data received so far.
            state.sb.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));

            if (bytesRead >= StateObject.BufferSize)
            {
                 //Get the rest of the data.
                handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
            else
            {
                if (state.sb.Length > 1)
                    ProcessRequest(state, handler);
            }            
        }

        private static void ProcessRequest(StateObject state, Socket socket)
        {
            var data = state.sb.ToString();
            // EOR = end of request
            var messages = data.Split(new string[] { General.EOR }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var message in messages)
            {
                var messageArray = message.Split(new char[] { General.CommandDelim }, 2);
                var commandType = messageArray[0];

                switch (commandType)
                {
                    case CommandType.Login:
                        {
                        TryConnect(socket, messageArray[1].Deserialize<User>());
                            break;
                        }
                    case CommandType.Subscribe:
                        {
                        Subscribe(socket, messageArray[1].Deserialize<User>());
                            break;
                        }
                    case CommandType.Logout:
                        {
                        Logout(socket);
                            break;
                        }
                    case CommandType.EditProfile:
                        {
                        EditProfile(socket, messageArray[1].Deserialize<Profile>());
                            break;
                        }
                    case CommandType.ViewProfile:
                        {
                            ViewProfile(socket, messageArray[1]);
                            break;
                        }
                    case CommandType.CreateRoom:
                        {
                            CreateRoom(socket, messageArray[1].Deserialize<Room>());
                            break;
                        }
                    case CommandType.JoinRoom:
                        {
                        JoinRoom(socket, Convert.ToInt32(messageArray[1]));
                            break;
                        }
                    case CommandType.LeaveRoom:
                        {
                        LeaveRoom(socket, Convert.ToInt32(messageArray[1]));
                        UpdateLobby(socket, onlineClients[socket]);
                            break;
                        }
                    case CommandType.SendMessage:
                        {
                            SendMessage(messageArray[1].Deserialize<Message>());
                            break;
                        }
                    case CommandType.DeleteMessage:
                        {
                            DeleteMessage(messageArray[1].Deserialize<Message>());
                            break;
                        }
                    case CommandType.SendLike:
                        {
                            SendLike(messageArray[1].Deserialize<Like>());
                            break;
                        }
                    default:
                        throw new Exception();
                }
            }
            state.sb.Clear();
            socket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                        ReadCallback, state);
        }

        public static void Send(Socket handler, string commandType, string data)
        {
            data = commandType + General.CommandDelim + data + General.EOR;
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
        /// <param name="socket"></param>
        /// <param name="user"></param>
        private static void TryConnect(Socket socket, User user)
        {
            foreach (var pair in onlineClients)
	        {
                if (user.Pseudo == ((Profile)pair.Value).Pseudo)
                {
                    Send(socket, CommandType.Error, "Usager déjà connecté ailleur");
                    Send(socket, CommandType.LoginAnswer, "False");
                    return;
                }
	        }

            if (users.Find(x => x.Pseudo == user.Pseudo && x.Password == user.Password) == null)
            {
                Send(socket, CommandType.Error, "Nom d'usager ou mot de passe invalide");
                Send(socket, CommandType.LoginAnswer, "False");
                return;
            }

            var profile = profiles.Find(x => x.Pseudo == user.Pseudo);
            onlineClients[socket] = profile;
            UpdateAllLobby();
            //UpdateLobby(socket, profile);
            Send(socket, CommandType.LoginAnswer, "True");
        }

        /// <summary>
        /// Créer un profile vide en remplissant juste le Pseudo, ajoute le user à la liste et retourne le profil
        /// au client pour qu'il puisse le compléter
        /// </summary>
        /// <param name="handlersocket"></param>
        /// <param name="user"></param>
        private static void Subscribe(Socket socket, User user)
        {
            if (users.Find(x => x.Pseudo == user.Pseudo) != null)
            {
                Send(socket, CommandType.Error, "Nom d'usager déjà existant");
                Send(socket, CommandType.SubscribeAnswer, "False");
                return;
            }
            var bidon = new Profile { Pseudo = user.Pseudo, IDRoom = -1 };
            profiles.Add(bidon);
            onlineClients[socket] = bidon;
            users.Add(user);
            UpdateLobby(socket, bidon);
            Send(socket, CommandType.SubscribeAnswer, "True");
        }

        /// <summary>
        /// Enleve le tuple du dictionaire en fonction du socket
        /// </summary>
        /// <param name="socket"></param>
        private static void Logout(Socket socket)
        {
            onlineClients.Remove(socket);
            UpdateAllLobby();
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
            //profiles[profiles.IndexOf(profile)] = newProfile;
            //onlineClients[socket].Avatar = newProfile.Avatar;
            onlineClients[socket].FirstName = newProfile.FirstName;
            onlineClients[socket].LastName = newProfile.LastName;
            Send(socket, CommandType.Info, "Le profil a été mis à jour");
            Send(socket, CommandType.EditProfileAnswer, "True");
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
            Send(socket, CommandType.ViewProfile, serializedProfile);
        }

        /// <summary>
        /// Ajoute la salle à la liste et retourne la salle
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="room"></param>
        private static void CreateRoom(Socket handler, Room room)
        {
            //leave room if he is already in one and create a new one
            if (onlineClients[handler].IDRoom != -1)
                LeaveRoom(handler, onlineClients[handler].IDRoom);

            //create a new room and affect only name, description and correct ID.
            if (rooms.Count > 0)
                room.IDRoom = rooms.Max(x => x.IDRoom) + 1;
            else
                room.IDRoom = 0;

            rooms.Add(room);
            //put the user in this room => updateRoom and updateLobby
            JoinRoom(handler, room.IDRoom);
            UpdateAllLobby();
            //UpdateLobby(handler, onlineClients[handler]);
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
            foreach(var profile in room.SubscribedUsers)
            {
                var s = onlineClients.FirstOrDefault(x => x.Value == profile).Key;
                Send(s, CommandType.UpdateRoom, room.Serialize()); 
            }
            UpdateLobby(socket, onlineClients[socket]);
        }

        /// <summary>
        /// Met à jour le id de la salle du profil à -1
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="idRoom"></param>
        private static void LeaveRoom(Socket handler, int idRoom)
        {
            var room = rooms.Find(x => x.IDRoom == idRoom);
            room.SubscribedUsers.Remove(onlineClients[handler]);
            if (room.SubscribedUsers.Count <= 0)
            {
                room.IsDeleted = true;
                UpdateAllLobby();
            }
                

            onlineClients[handler].IDRoom = -1;
        }

        /// <summary>
        /// Trouve les profils reliés au id de la salle de discussion. Trouve ensuite les socket reliés à chaque profil
        /// dans le dictionnaire onlineClients. Envoie finalement la salle à tous les sockets trouvés.
        /// </summary>
        /// <param name="message"></param>
        public static void SendMessage(Message message)
        {
            if (messages.Count > 0)
                message.IDMessage = messages.Max(x => x.IDMessage) + 1;
            else
                message.IDMessage = 0;

            // Ajoute à la liste de message du serveur
            messages.Add(message);
            var room = rooms.Find(x => x.IDRoom == message.IDRoom);
            // Ajoute à la liste de message
            room.Messages.Add(message);
            UpdateRoom(room);
        }

        /// <summary>
        /// Trouve les profils reliés au id de la salle de discussion. Trouve ensuite les socket reliés à chaque profil
        /// dans le dictionnaire onlineClients. Envoie finalement la salle à tous les sockets trouvés.
        /// </summary>
        /// <param name="message"></param>
        public static void DeleteMessage(Message message)
        {
            var message1 = messages.Find(x => x.Pseudo == message.Pseudo);
            message1.IsDeleted = true;
            var room = rooms.Find(x => x.IDRoom == message.IDRoom);
            var message2 = room.Messages.First(x => x.IDMessage == message.IDMessage);
            message2.IsDeleted = true;
            UpdateRoom(room);
        }

        /// <summary>
        /// Ajoute le like à la liste
        /// </summary>
        /// <param name="like"></param>
        private static void SendLike(Like like)
        {
            likes.Add(like);
        }

        /// <summary>
        /// Calcul le nombre de like pour chaque message et envoie ensuite la
        /// salle à tous les clients via leur socket
        /// </summary>
        /// <param name="room"></param>
        private static void UpdateRoom(Room room)
        {
            // Update nblike for each message
            foreach (var message in room.Messages)
            {
                if (room.IDRoom != message.IDRoom) continue;
                foreach (var like in likes.Where(like => like.IDMessage == message.IDMessage))
                {
                    if (like.IsLike)
                        message.NbLike++;
                    else
                        message.NbDislike++;
                }
            }

            List<Socket> listSockets = onlineClients.Where(x => room.SubscribedUsers.Any(p => p == x.Value)).Select(x => x.Key).ToList();

            foreach (var socket in listSockets)
            {
                Send(socket, CommandType.UpdateRoom, room.Serialize());
            }
        }

        private static void UpdateAllLobby()
        {
            foreach (var client in onlineClients)
                Send(client.Key, CommandType.UpdateLobby, lobby.Serialize());
        }
        private static void UpdateLobby(Socket socket, Profile profile)
        {
            semaphoreLobby.WaitOne();
            lobby.AllRooms = new ObservableCollection<Room>(rooms.Where(x => !x.IsDeleted));
            lobby.ClientProfile = profile;
            lobby.OtherUsers = new ObservableCollection<Profile>(profiles.Where(x => x.Pseudo != profile.Pseudo));
            Send(socket, CommandType.UpdateLobby, lobby.Serialize());
            semaphoreLobby.Release();
        }
    }
}
