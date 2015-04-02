using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //Charge les listes avant de commencer à écouter
            ChatServer.LoadServerInfos();

            //Démarre un thread pour sauvegarder les listes automatiquement
            Thread serverInfosThread = new Thread(ChatServer.ServerInfosTimer);
            serverInfosThread.Start();
            
            //Démarre un thread pour faire les mises à jour des lobbys
            Thread updateLobbyThread = new Thread(ChatServer.UpdateLobbyTimer);
            updateLobbyThread.Start();

            ChatServer.StartListening();
            
            //Juste au cas qu'on se rende ici :P
            serverInfosThread.Abort();
            updateLobbyThread.Abort();
        }
    }
}
