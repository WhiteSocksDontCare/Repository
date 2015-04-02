using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Mvvm;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;

namespace ChatClient.ViewModels
{
    class RoomViewModel : BindableBase
    {
        RoomViewModel() 
        {
            LeaveRoomCommand = new DelegateCommand(LeaveRoom);
            SendMessageCommand = new DelegateCommand(SendMessage);
        }
        public ICommand LeaveRoomCommand { get; private set; }
        public ICommand SendMessageCommand { get; private set; }


        public void LeaveRoom()
        {
            //TODO: Cacher l'envoie de message
        }

        public void SendMessage()
        {
            //TODO: Envoyer le message
        }
    }
}
