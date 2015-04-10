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

    class ChatServer
    {
        //Jajoute un 0 pour pas se faire spammer en debug
        private static double UPDATE_INTERVAL = 10000;  //Intervalle de temps entre deux mises à jour des lobbys vers les clients (10 secondes)
        private static double SAVE_INTERVAL = 90000;  //Intervalle de temps entre deux save des listes dans le fichier XML (1.5 minutes)
        private static string PROFILES_FILE = "profiles.xml";
        private static string ROOMS_FILE = "rooms.xml";
        private static string LIKES_FILE = "likes.xml";
        private static string USERS_FILE = "users.xml";
        private static string MESSAGES_FILE = "messages.xml";

        private static Semaphore _semaphoreOnlineClients = new Semaphore(1, 1);
        private static Semaphore _semaphoreProfiles = new Semaphore(1, 1);
        private static Semaphore _semaphoreRooms = new Semaphore(1, 1);
        private static Semaphore _semaphoreLikes = new Semaphore(1, 1);
        private static Semaphore _semaphoreUsers = new Semaphore(1, 1);
        private static Semaphore _semaphoreMessages = new Semaphore(1, 1);
        private static Semaphore _semaphoreLobby = new Semaphore(1, 1);

        private static Socket _listener = null;
        public static ManualResetEvent _allDone = new ManualResetEvent(false);
        private static Dictionary<Socket, Profile> _onlineClients = new Dictionary<Socket, Profile>();
        private static List<Profile> _profiles = new List<Profile>();
        private static List<Room> _rooms = new List<Room>();
        private static List<Like> _likes = new List<Like>();
        private static List<User> _users = new List<User>();
        private static List<Message> _messages = new List<Message>();
        private static Lobby _lobby = new Lobby();

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

            foreach (var client in _onlineClients)
            {
                client.Value.IsConnected = false;
                client.Key.Shutdown(SocketShutdown.Both);
                client.Key.Disconnect(false);
                client.Key.Close();
            }

            foreach(var room in _rooms)
            {
                room.SubscribedUsers = new ObservableCollection<Profile>();
            }

            ServerInfosTimerElapsed(null, null);
        }

        public static void LoadServerInfos()
        {
            _semaphoreProfiles.WaitOne();
            List<Profile> tempProfiles;
            if ((tempProfiles = ChatCommunication.SerializerHelper.DeserializeFromXML<List<Profile>>(PROFILES_FILE)) != null)
                _profiles = tempProfiles;
            _semaphoreProfiles.Release();

            _semaphoreRooms.WaitOne();
            List<Room> tempRooms;
            if ((tempRooms = ChatCommunication.SerializerHelper.DeserializeFromXML<List<Room>>(ROOMS_FILE)) != null)
                _rooms = tempRooms;
            _semaphoreRooms.Release();

            _semaphoreLikes.WaitOne();
            List<Like> tempLikes;
            if ((tempLikes = ChatCommunication.SerializerHelper.DeserializeFromXML<List<Like>>(LIKES_FILE)) != null)
                _likes = tempLikes;
            _semaphoreLikes.Release();

            _semaphoreUsers.WaitOne();
            List<User> tempUsers;
            if ((tempUsers = ChatCommunication.SerializerHelper.DeserializeFromXML<List<User>>(USERS_FILE)) != null)
                _users = tempUsers;
            _semaphoreUsers.Release();

            _semaphoreMessages.WaitOne();
            List<Message> tempMessages;
            if ((tempMessages = ChatCommunication.SerializerHelper.DeserializeFromXML<List<Message>>(MESSAGES_FILE)) != null)
                _messages = tempMessages;
            _semaphoreMessages.Release();
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
            _semaphoreProfiles.WaitOne();
            ChatCommunication.SerializerHelper.SerializeToXML(_profiles, PROFILES_FILE);
            _semaphoreProfiles.Release();

            _semaphoreRooms.WaitOne();
            ChatCommunication.SerializerHelper.SerializeToXML(_rooms, ROOMS_FILE);
            _semaphoreRooms.Release();

            _semaphoreLikes.WaitOne();
            ChatCommunication.SerializerHelper.SerializeToXML(_likes, LIKES_FILE);
            _semaphoreLikes.Release();

            _semaphoreUsers.WaitOne();
            ChatCommunication.SerializerHelper.SerializeToXML(_users, USERS_FILE);
            _semaphoreUsers.Release();

            _semaphoreMessages.WaitOne();
            ChatCommunication.SerializerHelper.SerializeToXML(_messages, MESSAGES_FILE);
            _semaphoreMessages.Release();
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
            //_semaphoreOnlineClients.WaitOne();

            UpdateAllLobby();

            //_semaphoreOnlineClients.Release();
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
                    _allDone.Reset();

                    Console.WriteLine("Waiting for someone...");
                    if (_listener == null)
                        return;
                    _listener.BeginAccept(AcceptCallback, _listener);
                    _allDone.WaitOne();
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
            _allDone.Set();
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
            state.sb.Append(Encoding.UTF8.GetString(state.Buffer, 0, bytesRead));

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

                            _semaphoreOnlineClients.WaitOne();
                            var profile = _onlineClients[socket];
                            _semaphoreOnlineClients.Release();

                            UpdateLobby(socket, profile);
                            break;
                        }
                    case CommandType.SendMessage:
                        {
                            SendMessage(messageArray[1].Deserialize<Message>());
                            break;
                        }
                    case CommandType.DeleteMessage:
                        {
                            DeleteMessage(Convert.ToInt32(messageArray[1]));
                            break;
                        }
                    case CommandType.SendLike:
                        {
                            SendLike(socket, messageArray[1].Deserialize<Like>());
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
            var byteData = Encoding.UTF8.GetBytes(data);

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
            _semaphoreOnlineClients.WaitOne();
            foreach (var pair in _onlineClients)
            {
                if (user.Pseudo == ((Profile)pair.Value).Pseudo)
                {
                    Send(socket, CommandType.Error, "User is already connected");
                    Send(socket, CommandType.LoginAnswer, "False");
                    _semaphoreOnlineClients.Release();
                    return;
                }
            }
            _semaphoreOnlineClients.Release();

            _semaphoreUsers.WaitOne();
            if (_users.Find(x => x.Pseudo == user.Pseudo && x.Password == user.Password) == null)
            {
                Send(socket, CommandType.Error, "Invalid pseudo or password.");
                Send(socket, CommandType.LoginAnswer, "False");
                _semaphoreUsers.Release();
                return;
            }
            _semaphoreUsers.Release();

            _semaphoreProfiles.WaitOne();
            var profile = _profiles.Find(x => x.Pseudo == user.Pseudo);
            _semaphoreProfiles.Release();

            _semaphoreOnlineClients.WaitOne();
            _onlineClients[socket] = profile;
            _onlineClients[socket].IsConnected = true;
            _semaphoreOnlineClients.Release();

            UpdateAllLobby();

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
            _semaphoreUsers.WaitOne();
            if (_users.Find(x => x.Pseudo == user.Pseudo) != null)
            {
                Send(socket, CommandType.Error, "Pseudo already existing");
                Send(socket, CommandType.SubscribeAnswer, "False");
                _semaphoreUsers.WaitOne();
                return;
            }
            _semaphoreUsers.Release();

            var bidon = new Profile { Pseudo = user.Pseudo, IDRoom = -1, IsConnected= true };
            _semaphoreProfiles.WaitOne();
            _profiles.Add(bidon);
            _semaphoreProfiles.Release();

            _semaphoreOnlineClients.WaitOne();
            _onlineClients[socket] = bidon;
            _semaphoreOnlineClients.Release();

            _semaphoreUsers.WaitOne();
            _users.Add(user);
            _semaphoreUsers.Release();

            UpdateAllLobby();
            Send(socket, CommandType.SubscribeAnswer, "True");
        }

        /// <summary>
        /// Enleve le tuple du dictionaire en fonction du socket
        /// </summary>
        /// <param name="socket"></param>
        private static void Logout(Socket socket)
        {
            _semaphoreOnlineClients.WaitOne();
            _onlineClients[socket].IsConnected = false;
            _onlineClients.Remove(socket);
            _semaphoreOnlineClients.Release(); 
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
            _semaphoreProfiles.WaitOne();
            var profile = _profiles.Find(x => x.Pseudo == newProfile.Pseudo);
            _semaphoreProfiles.Release();

            _semaphoreOnlineClients.WaitOne();
            _onlineClients[socket].FirstName = newProfile.FirstName;
            _onlineClients[socket].LastName = newProfile.LastName;
            _onlineClients[socket].AvatarUri = newProfile.AvatarUri;
            _semaphoreOnlineClients.Release();
            Send(socket, CommandType.Info, "The profile has been updated");
            Send(socket, CommandType.EditProfileAnswer, "True");
        }

        /// <summary>
        /// Trouve le profile correspondant au nom d'usager et le retourne au client
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="username"></param>
        private static void ViewProfile(Socket socket, string username)
        {
            _semaphoreProfiles.WaitOne();
            var profile = _profiles.Find(x => x.Pseudo == username);
            UpdateStats(profile);
            _semaphoreProfiles.Release();

            var serializedProfile = profile.Serialize();
            Send(socket, CommandType.ViewProfile, serializedProfile);
        }

        /// <summary>
        /// Ajoute la salle à la liste et retourne la salle
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="room"></param>
        private static void CreateRoom(Socket socket, Room room)
        {
            //create a new room and affect only name, description and correct ID.
            _semaphoreRooms.WaitOne();
            if (_rooms.Count > 0)
                room.IDRoom = _rooms.Max(x => x.IDRoom) + 1;
            else
                room.IDRoom = 0;

            _rooms.Add(room);
            _semaphoreRooms.Release();

            //put the user in this room => updateRoom and updateLobby
            JoinRoom(socket, room.IDRoom);
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
            if (_onlineClients[socket].IDRoom == idRoom)
            {
                return;
            }

            //leave room if he is already in one and create a new one
            _semaphoreOnlineClients.WaitOne();
            int id = _onlineClients[socket].IDRoom ;
            _semaphoreOnlineClients.Release();

            if (id != -1)
                LeaveRoom(socket, id);

            _semaphoreRooms.WaitOne();
            var room = _rooms.Find(x => x.IDRoom == idRoom);
            _semaphoreRooms.Release();

            _semaphoreOnlineClients.WaitOne();
            _onlineClients[socket].IDRoom = idRoom;            
            room.SubscribedUsers.Add(_onlineClients[socket]);
            UpdateLobby(socket, _onlineClients[socket]);
            _semaphoreOnlineClients.Release();

            _semaphoreOnlineClients.WaitOne();
            foreach (var profile in room.SubscribedUsers)
            {
                var s = _onlineClients.FirstOrDefault(x => x.Value == profile).Key;
                Send(s, CommandType.UpdateRoom, room.Serialize());
            }
            _semaphoreOnlineClients.Release();
        }

        /// <summary>
        /// Met à jour le id de la salle du profil à -1
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="idRoom"></param>
        private static void LeaveRoom(Socket handler, int idRoom)
        {
            _semaphoreRooms.WaitOne();
            var room = _rooms.Find(x => x.IDRoom == idRoom);
            room.SubscribedUsers.Remove(_onlineClients[handler]);
            int count = room.SubscribedUsers.Count;
            _semaphoreRooms.Release();

            if (count <= 0)
            {
                _semaphoreRooms.WaitOne();
                room.IsDeleted = true;
                _semaphoreRooms.Release();
                UpdateAllLobby();
            }
            else
            {
                UpdateRoom(room);
            }
            _semaphoreOnlineClients.WaitOne();
            _onlineClients[handler].IDRoom = -1;
            _semaphoreOnlineClients.Release();
        }

        /// <summary>
        /// Trouve les profils reliés au id de la salle de discussion. Trouve ensuite les socket reliés à chaque profil
        /// dans le dictionnaire onlineClients. Envoie finalement la salle à tous les sockets trouvés.
        /// </summary>
        /// <param name="message"></param>
        public static void SendMessage(Message message)
        {
            _semaphoreMessages.WaitOne();
            if (_messages.Count > 0)
                message.IDMessage = _messages.Max(x => x.IDMessage) + 1;
            else
                message.IDMessage = 0;

            // Ajoute à la liste de message du serveur
            _messages.Add(message);
            _semaphoreMessages.Release();

            _semaphoreRooms.WaitOne();
            var room = _rooms.Find(x => x.IDRoom == message.IDRoom);
            // Ajoute à la liste de message
            room.Messages.Add(message);
            UpdateRoom(room);
            _semaphoreRooms.Release();
        }

        /// <summary>
        /// Trouve les profils reliés au id de la salle de discussion. Trouve ensuite les socket reliés à chaque profil
        /// dans le dictionnaire onlineClients. Envoie finalement la salle à tous les sockets trouvés.
        /// </summary>
        /// <param name="message"></param>
        public static void DeleteMessage(int messageID)
        {
            _semaphoreMessages.WaitOne();
            var msg = _messages.Find(x => x.IDMessage == messageID);

            if (msg == null)
            {
                _semaphoreMessages.Release();
                return;
            }

            msg.IsDeleted = true;
            _semaphoreMessages.Release();

            _semaphoreRooms.WaitOne();
            var room = _rooms.Find(x => x.IDRoom == msg.IDRoom);
            room.Messages.Remove(msg);
            UpdateRoom(room);
            _semaphoreRooms.Release();
        }

        /// <summary>
        /// Ajoute le like à la liste
        /// </summary>
        /// <param name="like"></param>
        private static void SendLike(Socket socket, Like like)
        {
            
            like.Pseudo = _onlineClients[socket].Pseudo;
            _semaphoreLikes.WaitOne();

            var l = _likes.Find(x => x.Pseudo == like.Pseudo && x.IDMessage == like.IDMessage);
            if (l == null)
                _likes.Add(like);
            else
                l.IsLike = like.IsLike;
            _semaphoreLikes.Release();

            _semaphoreRooms.WaitOne();
            var room = _rooms.First(x => x.IDRoom == _onlineClients[socket].IDRoom);
            UpdateRoom(room);
            _semaphoreRooms.Release();
        }

        /// <summary>
        /// Calcul le nombre de like pour chaque message et envoie ensuite la
        /// salle à tous les clients via leur socket
        /// </summary>
        /// <param name="room"></param>
        private static void UpdateRoom(Room room)
        {
            // Update nblike for each message
            foreach (var message in room.Messages.Where(x => !x.IsDeleted && x.IDRoom == room.IDRoom))
            {
                //_semaphoreLikes.WaitOne();
                message.NbLike = _likes.Count(x => x.IDMessage == message.IDMessage && x.IsLike);
                message.NbDislike = _likes.Count(x => x.IDMessage == message.IDMessage && !x.IsLike);
                //_semaphoreLikes.Release();
            }

            _semaphoreOnlineClients.WaitOne();
            List<Socket> listSockets = _onlineClients.Where(x => room.SubscribedUsers.Any(p => p == x.Value)).Select(x => x.Key).ToList();
            _semaphoreOnlineClients.Release();

            foreach (var socket in listSockets)
            {
                Send(socket, CommandType.UpdateRoom, room.Serialize());
            }
        }

        private static void UpdateAllLobby()
        {
            _semaphoreOnlineClients.WaitOne();
            foreach (var client in _onlineClients)
                UpdateLobby(client.Key, client.Value);
            _semaphoreOnlineClients.Release();
        }
        private static void UpdateLobby(Socket socket, Profile profile)
        {
            _semaphoreLobby.WaitOne();

            _semaphoreRooms.WaitOne();
            _lobby.AllRooms = new ObservableCollection<Room>(_rooms.Where(x => !x.IsDeleted));
            _semaphoreRooms.Release();

            _lobby.ClientProfile = profile;
            UpdateStats(profile);

            _semaphoreProfiles.WaitOne();
            _lobby.OtherUsers = new ObservableCollection<Profile>(_profiles.Where(x => x.Pseudo != profile.Pseudo));
            _semaphoreProfiles.Release();

            Send(socket, CommandType.UpdateLobby, _lobby.Serialize());
            _semaphoreLobby.Release();
        }

        private static void UpdateStats(Profile profile)
        {
            profile.NbMessage = _messages.Count(x => x.Pseudo == profile.Pseudo);
            profile.NbDeletedMessage = _messages.Count(x => x.Pseudo == profile.Pseudo && x.IsDeleted);
            profile.NbLike = 0;
            profile.NbDislike = 0;
            foreach (var message in _messages.Where(x => x.Pseudo == profile.Pseudo))
            {
                profile.NbLike += _likes.Count(x => x.IDMessage == message.IDMessage && x.IsLike);
                profile.NbDislike += _likes.Count(x => x.IDMessage == message.IDMessage && !x.IsLike);
            }
        }
    }
}
