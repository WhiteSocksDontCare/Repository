using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ChatServer
{
    class Program
    {
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType sig)
        {
            //switch (sig)
            //{
            //    case CtrlType.CTRL_C_EVENT:
            //    case CtrlType.CTRL_LOGOFF_EVENT:
            //    case CtrlType.CTRL_SHUTDOWN_EVENT:
            //    case CtrlType.CTRL_CLOSE_EVENT:
            //    default:
            //        return false;
            //}
            ChatServer.CleanUp();
            return true;
        }
        static void Main(string[] args)
        {
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

            //Charge les listes avant de commencer à écouter
            ChatServer.LoadServerInfos();

            //Démarre un thread pour sauvegarder les listes automatiquement
            //Thread serverInfosThread = new Thread(ChatServer.ServerInfosTimer);
            //serverInfosThread.Start();

            //Démarre un thread pour faire les mises à jour des lobbys
            Thread updateLobbyThread = new Thread(ChatServer.UpdateLobbyTimer);
            updateLobbyThread.Start();

            ChatServer.StartListening();

            //Juste au cas qu'on se rende ici :P
            //serverInfosThread.Abort();
            updateLobbyThread.Abort();
        }
    }
}
