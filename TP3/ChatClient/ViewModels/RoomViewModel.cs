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
using System.Windows.Controls;

namespace ChatClient.ViewModels
{
    class RoomViewModel : BindableBase
    {
        private Room _room;
        private Message _message;
        //private readonly Lazy<ObservableCollection<MessageViewModel>> _messages;
        //private readonly Lazy<ObservableCollection<ProfileViewModel>> _subscribedUsers;

        public RoomViewModel()
        {
            LeaveRoomCommand = new DelegateCommand(LeaveRoom);
            SendMessageCommand = new DelegateCommand(SendMessage);
            LikeCommand = new DelegateCommand<Message>(LikeMessage);
            DislikeCommand = new DelegateCommand<Message>(DislikeMessage);
            DeleteCommand = new DelegateCommand<Message>(DeleteMessage);

            ////Alex: test de creation d'une ObservableCollection sync entre la vue et le model
            ////_messages
            //Func<Message, MessageViewModel> messageViewModelCreator = model => new MessageViewModel() { Message = model };
            //Func<ObservableCollection<MessageViewModel>> messageCollectionCreator =
            //    () => new ObservableViewModelCollection<MessageViewModel, Message>(Room.Messages, messageViewModelCreator);
            //_messages = new Lazy<ObservableCollection<MessageViewModel>>(messageCollectionCreator);

            ////_subscribedUsers
            //Func<Profile, ProfileViewModel> profileViewModelCreator = model => new ProfileViewModel() { Profile = model };
            //Func<ObservableCollection<ProfileViewModel>> profileCollectionCreator =
            //    () => new ObservableViewModelCollection<ProfileViewModel, Profile>(Room.SubscribedUsers, profileViewModelCreator);
            //_subscribedUsers = new Lazy<ObservableCollection<ProfileViewModel>>(profileCollectionCreator);

            _room = new Room();
            _message = new Message();
        }

        //public ObservableCollection<MessageViewModel> MessageViewModels
        //{
        //    get { return _messages.Value; }
        //}

        //public ObservableCollection<ProfileViewModel> ProfileViewModels
        //{
        //    get { return _subscribedUsers.Value; }
        //}

        public Room Room
        {
            get { return _room; }
            set { SetProperty(ref _room, value); }
        }
        public Message Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }


        public ICommand LeaveRoomCommand { get; private set; }
        public ICommand SendMessageCommand { get; private set; }
        public ICommand LikeCommand { get; private set; }
        public ICommand DislikeCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }


        public void LeaveRoom()
        {
            Client.LeaveRoom(Room.IDRoom);
        }

        public void SendMessage()
        {
            Message.IDRoom = Room.IDRoom;
            Message.Pseudo = Container.GetA<LobbyViewModel>().Lobby.ClientProfile.Pseudo;
            Client.SendMessage(Message);
            Message.Text = "";
        }

        public void LikeMessage(Message msg)
        {
            Like like = new Like { IDMessage = msg.IDMessage, IsLike = true };
            Client.SendLike(like);
        }

        public void DislikeMessage(Message msg)
        {
            Like like = new Like{IDMessage = msg.IDMessage, IsLike = false};
            Client.SendLike(like);
        }

        public void DeleteMessage(Message msg)
        {
            Client.DeleteMessage(msg.IDMessage);
        }
    }
}
