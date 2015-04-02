using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using ChatCommunication;
using Microsoft.Practices.Prism.Commands;
using MVVM.Container;

namespace ChatClient.ViewModels
{
    class LobbyViewModel : BindableBase
    {
        private bool _isInRoom;
        private List<User> _users;
        private List<Room> _rooms; 
 
        public bool IsInRoom
        {
            get { return _isInRoom; }
            set { SetProperty(ref _isInRoom, value); }
        }
        public ICommand DisconnectCommand { get; private set; }
        public ICommand EditProfileCommand { get; private set; }
        public ICommand LeaveRoomCommand { get; private set; }
        public ICommand SendMessageCommand { get; private set; }

        public LobbyViewModel()
        {
            DisconnectCommand = new DelegateCommand(Disconnect);
            EditProfileCommand = new DelegateCommand(EditProfile);
            LeaveRoomCommand= new DelegateCommand(LeaveRoom);
            SendMessageCommand= new DelegateCommand(SendMessage);
        }

        public void Disconnect()
        {
            Container.GetA<MainViewModel>().NavigateToView(Container.GetA<LoginViewModel>());
        }

        public void EditProfile()
        {
            Container.GetA<MainViewModel>().NavigateToView(Container.GetA<ProfileViewModel>());
        }

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
