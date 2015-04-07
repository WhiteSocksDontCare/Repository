using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Xml.Serialization;
using ChatCommunication;
using System.Windows;
using MVVM.Container;
using ChatClient.ViewModels;

namespace ChatClient
{
    public class StateObject
    {
        // Client socket.
        public Socket WorkSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] Buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    public class Client
    {
        private const int port = 11000;

        private static readonly ManualResetEvent connectDone = new ManualResetEvent(false);
        private static readonly ManualResetEvent sendDone = new ManualResetEvent(false);
        private static readonly AutoResetEvent receiveDone = new AutoResetEvent(false);

        private static bool response;
        private static Socket client = null;
        private static Thread threadlisten;

        public static bool IsConnected()
        {
            return client != null;
        }
        public static bool EstablishConnection()
        {
             try
            {
                var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = null;
                foreach (var addr in Dns.GetHostEntry(string.Empty).AddressList.Where(addr => addr.AddressFamily == AddressFamily.InterNetwork))
                {
                    ipAddress = addr;
                }
                var remoteEP = new IPEndPoint(ipAddress, port);

                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                
                client.BeginConnect(remoteEP, ConnectCallback, client);
                connectDone.WaitOne();

                threadlisten = new Thread(new ThreadStart(listening));
                threadlisten.Start();

                return true;
            }
             catch (Exception ex)
             {
                 Console.WriteLine(ex.ToString());
             }
             return false;
        }

        public static void listening()
        {
            while (true)
            {
                try
                {
                    Receive();
                    receiveDone.WaitOne();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public static void LogClient(User user)
        {
            try
            {
                Send("Login", user.Serialize());
                sendDone.WaitOne();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                
            }
        }
        public static void SubClient(User user)
        {
            try
            {
                Send("Subscribe", user.Serialize());
                sendDone.WaitOne();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static void UpdateProfile(Profile profile)
        {
            try
            {
                Send("EditProfile", profile.Serialize());
                sendDone.WaitOne();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static void ViewProfile(string Pseudo)
        {
            try
            {
                Send("ViewProfile", Pseudo);
                sendDone.WaitOne();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static void CreateRoom(Room room)
        {
            try
            {
                Send("CreateRoom", room.Serialize());
                sendDone.WaitOne();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static bool DisconnectClient()
        {
            try
            {
                client.Shutdown(SocketShutdown.Both);
                client.Disconnect(true);
                threadlisten.Abort();
                threadlisten.Join();
                client.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket) ar.AsyncState;

                client.EndConnect(ar);

                Console.WriteLine(@"Socket connected to {0}", client.RemoteEndPoint);

                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Receive()
        {
            try
            {
                var state = new StateObject {WorkSocket = client};

                client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            var state = (StateObject)ar.AsyncState;
            var handler = state.WorkSocket;

            // Read data from the client socket. 
            var bytesRead = handler.EndReceive(ar);

            
            // There might be more data, so store the data received so far.
            state.sb.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));

            if (bytesRead >= StateObject.BufferSize)
            {    
                // Get the rest of the data.
                client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            else
            {
                if (state.sb.Length > 1)
                {
                    Console.WriteLine(state.sb.ToString());
                    ExecuteMessage(state.sb.ToString());
                    receiveDone.Set();
                    state.sb.Clear();
                }
            }
        }

        private static void ExecuteMessage(string message)
        {
            var messageArray = message.Split(new char[] { '!' }, 2);
            var commandType = messageArray[0];
            bool result;
            Console.WriteLine(commandType);
            response = true;

            switch (commandType)
            {
                case "Info":
                    var messageInfo = messageArray[1];
                    MessageBox.Show(messageInfo, "Informations", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case "Error":
                    response = false;
                    var messageError = messageArray[1];
                    MessageBox.Show(messageError, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                //UpdateLobby -> cas general. recu en tout temps; profil vide lors de la creation
                case "UpdateLobby":
                    Lobby lobby = messageArray[1].Deserialize<Lobby>();
                    Container.GetA<LobbyViewModel>().Lobby = lobby;
                    break;
                //UpdateRoom -> Recu seulement quand le client est dans une room
                case "UpdateRoom":
                    Room room = messageArray[1].Deserialize<Room>();
                    Container.GetA<LobbyViewModel>().RoomViewModel.Room = room;
                    break;
                //UpdateProfile -> recu dans le cas d'une consultation de profil (sois-meme ou autre)
                case "ViewProfile":
                    Profile profile = messageArray[1].Deserialize<Profile>();
                    Container.GetA<ViewProfileViewModel>().Profile = profile;
                    Container.GetA<LobbyViewModel>().ViewProfileCallback();
                    break;
                //LoginAnswer -> recu pour savoir si le login a marcher ou pas
                case "LoginAnswer":
                    result = messageArray[1].Equals("True");
                    Container.GetA<LoginViewModel>().LoginCallback(result);
                    break;
                //SubscribeAnswer -> recu pour savoir si le Subscribe a marcher ou pas
                case "SubscribeAnswer":
                    result = messageArray[1].Equals("True");                 
                    Container.GetA<EditProfileViewModel>().Profile = Container.GetA<LobbyViewModel>().Lobby.ClientProfile;
                    Container.GetA<LoginViewModel>().SubscribeCallback(result);
                    break;
                //EditProfileAnswer -> recu pour savoir si le l'edition a marcher ou pas
                case "EditProfileAnswer":
                    result = messageArray[1].Equals("True");
                    Container.GetA<EditProfileViewModel>().EditProfileCallback(result);
                    break;
                default:
                    throw new Exception("Commande '" + commandType + "' non reconnue.");
            }
        }

        private static void Send(string commandType, string data)
        {
            data = commandType + "!"+ data;
            var byteData = Encoding.ASCII.GetBytes(data);

            client.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, client);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket) ar.AsyncState;

                var bytesSent = client.EndSend(ar);
                Console.WriteLine(@"Sent {0} bytes to server", bytesSent);

                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
