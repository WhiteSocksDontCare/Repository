using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Mvvm;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using ChatCommunication;
using MVVM.Container;
using System.Collections.ObjectModel;

namespace ChatClient.ViewModels
{
    class RoomViewModel : BindableBase
    {

        private Room _room;

        public RoomViewModel()
        {
            LeaveRoomCommand = new DelegateCommand(LeaveRoom);
            SendMessageCommand = new DelegateCommand(SendMessage);
            Room = new Room();
            ObservableCollection<Message> msg = new ObservableCollection<Message>(Room.Messages);
            Room.Messages
        }

        public Room Room
        {
            get { return _room; }
            set { SetProperty(ref _room, value); }
        }

        //Utile pour les élement d'une liste. car un message doit etre texte + like ou dislike
        public ObservableCollection<MessageViewModel> MessageViewModels
        {
            get { return Container.GetA<MessageViewModel>(); }
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
