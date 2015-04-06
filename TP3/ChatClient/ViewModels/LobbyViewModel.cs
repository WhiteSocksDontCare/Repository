using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        private Lobby _lobby;
        private Thread _updateLobby;
        private Profile _userProfile;
        private RoomViewModel _roomViewModel;


        public LobbyViewModel()
        {
            DisconnectCommand = new DelegateCommand(Disconnect);
            EditProfileCommand = new DelegateCommand(EditProfile);
            ViewProfileCommand = new DelegateCommand(ViewProfile);

            _updateLobby = new Thread(UpdateLobby);
            _updateLobby.Start();

            _lobby = new Lobby();
            _roomViewModel = new RoomViewModel();
        }

        public void UpdateLobby()
        {
            while (true)
            {
                Client.UpdateLobby();
            }
        }

        public Lobby Lobby
        {
            get { return _lobby; }
            set { SetProperty(ref _lobby, value); }
        }

        public RoomViewModel RoomViewModel
        {
            get { return _roomViewModel; }
        }
 
        public bool IsInRoom
        {
            get { return _isInRoom; }
            set { SetProperty(ref _isInRoom, value); }
        }
        public ICommand DisconnectCommand { get; private set; }
        public ICommand EditProfileCommand { get; private set; }
        public ICommand ViewProfileCommand { get; private set; }

        public void Disconnect()
        {
            Container.GetA<MainViewModel>().NavigateToView(Container.GetA<LoginViewModel>());
        }

        public void EditProfile()
        {
            Container.GetA<MainViewModel>().NavigateToView(Container.GetA<EditProfileViewModel>());
        }

        public void ViewProfile()
        {
            Container.GetA<MainViewModel>().NavigateToView(Container.GetA<ViewProfileViewModel>());
        }

    }
}
