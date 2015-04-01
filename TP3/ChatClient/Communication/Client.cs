﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Controls.Primitives;
using System.Xml.Serialization;
using ChatCommunication;
using System.Windows;

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

    class Client
    {
        private const int port = 11000;

        private static readonly ManualResetEvent connectDone = new ManualResetEvent(false);
        private static readonly ManualResetEvent sendDone = new ManualResetEvent(false);
        private static readonly ManualResetEvent receiveDone = new ManualResetEvent(false);

        private static String response = String.Empty;

        public static bool StartClient(User user)
        {
            try
            {
                var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                var ipAddress = ipHostInfo.AddressList[1];
                var remoteEP = new IPEndPoint(ipAddress, port);

                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                client.BeginConnect(remoteEP, ConnectCallback, client);
                connectDone.WaitOne(); 

                Send(client, "Login", user.Serialize());
                sendDone.WaitOne();

                Receive(client);
                receiveDone.WaitOne();
                
                Console.WriteLine(@"Response received : {0}", response);

                client.Shutdown(SocketShutdown.Both);
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

        private static void Receive(Socket client)
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

            if (bytesRead <= 0) return;

            var message = Encoding.ASCII.GetString(state.Buffer, 0, bytesRead);

            var messageArray = message.Split(new char[] { '!' }, 2);
            var commandType = messageArray[0];
            Console.WriteLine(commandType);
            
            switch (commandType)
            {
                case "Info":
                    var messageInfo = messageArray[1];
                    MessageBox.Show(messageInfo, "Informations", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case "Error":
                    var messageError = messageArray[1];
                    MessageBox.Show(messageError, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case "UpdateLobby":
                    Lobby lobby = messageArray[1].Deserialize<Lobby>();
                    // TODO : Mettre à jour le lobby.
                    break;
                case "UpdateRoom":                    
                    Room room = messageArray[1].Deserialize<Room>();
                    // TODO : Mettre à jour la room dans laquelle se trouve l'utilisateur.
                    break;
                case "UpdateProfile":
                    Profile profile = messageArray[1].Deserialize<Profile>();
                    // TODO : Afficher le profil reçu.
                    break;
                default:
                    throw new Exception("Commande '" + commandType + "' non reconnue.");
            }

            //try
            //{
            //    var state = (StateObject) ar.AsyncState;
            //    var client = state.workSocket;

            //    var bytesRead = client.EndReceive(ar);

            //    if (bytesRead > 0)
            //    {
            //        state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
            //        client.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0, ReceiveCallback, state);
            //    }
            //    else
            //    {
            //        if (state.sb.Length > 1)
            //            response = state.sb.ToString();

            //        receiveDone.Set();
            //    }
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.ToString());
            //}
        }

        private static void Send(Socket client, string commandType, string data)
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
