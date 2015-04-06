using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Mvvm;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using ChatCommunication;
using ChatCommunication.Extension;
using MVVM.Container;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using ChatClient.Utils;

namespace ChatClient.ViewModels
{
    class RoomViewModel : BindableBase
    {

        private Room _room;
        private readonly Lazy<ObservableCollection<MessageViewModel>> _messages;

        public RoomViewModel()
        {
            LeaveRoomCommand = new DelegateCommand(LeaveRoom);
            SendMessageCommand = new DelegateCommand(SendMessage);
            _room = new Room();

            //Alex: test de creation d'une ObservableCollection sync entre la vue et le model
            Func<Message, MessageViewModel> viewModelCreator = model => new MessageViewModel() { Message = model };
            Func<ObservableCollection<MessageViewModel>> collectionCreator =
                () => new ObservableViewModelCollection<MessageViewModel, Message>(Room.Messages, viewModelCreator);
            _messages = new Lazy<ObservableCollection<MessageViewModel>>(collectionCreator);
        }

        public ObservableCollection<MessageViewModel> MessageViewModels
        {
            get { return _messages.Value; }
        }

        public Room Room
        {
            get { return _room; }
            set { SetProperty(ref _room, value); }
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
