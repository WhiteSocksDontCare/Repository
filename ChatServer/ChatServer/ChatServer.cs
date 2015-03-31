using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;

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
        private static int i = 0;
        public static ManualResetEvent AllDone = new ManualResetEvent(false);
        private static Dictionary<string, Socket> clients = new Dictionary<string,Socket>();

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

            var commandType = (Type)Int32.Parse(Encoding.ASCII.GetString(state.Buffer, 0, 1));
            var message = Encoding.ASCII.GetString(state.Buffer, 1, bytesRead-1);

            if (commandType == Type.Login)
            {
                Console.WriteLine("login");
                clients[message] = handler;
                Send(clients[message], "salut");
            }
            
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
