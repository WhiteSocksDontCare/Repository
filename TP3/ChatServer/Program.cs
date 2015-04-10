using System.Runtime.InteropServices;
using System.Threading;

namespace ChatServer
{
    class Program
    {
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler();
        static EventHandler _handler;

        private static bool Handler()
        {
            ChatServer.CleanUp();
            return true;
        }
        static void Main()
        {
            _handler += Handler;
            SetConsoleCtrlHandler(_handler, true);

            //Charge les listes avant de commencer à écouter
            ChatServer.LoadServerInfos();

            //Démarre un thread pour sauvegarder les listes automatiquement
            var serverInfosThread = new Thread(ChatServer.ServerInfosTimer);
            serverInfosThread.Start();

            //Démarre un thread pour faire les mises à jour des lobbys
            var updateLobbyThread = new Thread(ChatServer.UpdateLobbyTimer);
            updateLobbyThread.Start();

            ChatServer.StartListening();

            //Juste au cas qu'on se rende ici :P
            serverInfosThread.Abort();
            updateLobbyThread.Abort();
        }
    }
}
