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
        private readonly Lazy<ObservableCollection<ProfileViewModel>> _subscribedUsers;

        public RoomViewModel()
        {
            LeaveRoomCommand = new DelegateCommand(LeaveRoom);
            SendMessageCommand = new DelegateCommand(SendMessage);
            _room = new Room();

            //Alex: test de creation d'une ObservableCollection sync entre la vue et le model
            //_messages
            Func<Message, MessageViewModel> messageViewModelCreator = model => new MessageViewModel() { Message = model };
            Func<ObservableCollection<MessageViewModel>> messageCollectionCreator =
                () => new ObservableViewModelCollection<MessageViewModel, Message>(Room.Messages, messageViewModelCreator);
            _messages = new Lazy<ObservableCollection<MessageViewModel>>(messageCollectionCreator);

            //_subscribedUsers
            Func<Profile, ProfileViewModel> profileViewModelCreator = model => new ProfileViewModel() { Profile = model };
            Func<ObservableCollection<ProfileViewModel>> profileCollectionCreator =
                () => new ObservableViewModelCollection<ProfileViewModel, Profile>(Room.SubscribedUsers, profileViewModelCreator);
            _subscribedUsers = new Lazy<ObservableCollection<ProfileViewModel>>(profileCollectionCreator);
        }

        public ObservableCollection<MessageViewModel> MessageViewModels
        {
            get { return _messages.Value; }
        }

        public ObservableCollection<ProfileViewModel> ProfileViewModels
        {
            get { return _subscribedUsers.Value; }
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
