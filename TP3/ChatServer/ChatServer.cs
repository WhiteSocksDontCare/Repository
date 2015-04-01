using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;
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

                    Console.WriteLine("Waiting for someone..." + i++);
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

            var state = new StateObject {WorkSocket = handler};

            handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                ReadCallback, state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            var state = (StateObject) ar.AsyncState;
            var handler = state.WorkSocket;

            // Read data from the client socket. 
            var bytesRead = handler.EndReceive(ar);

            if (bytesRead <= 0) return;

            var message = Encoding.ASCII.GetString(state.Buffer, 0, bytesRead);

            var messageArray = message.Split(new char[] {'!'}, 2);
            var commandType = messageArray[0];
            Console.WriteLine(commandType);
            clients[message] = handler;
            Send(clients[message], "salut");
            
        }

        public static void Send(Socket handler, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        public static void SendCallback(IAsyncResult ar)
        {
            try
            {
                var handler = (Socket) ar.AsyncState;

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
    }
}
