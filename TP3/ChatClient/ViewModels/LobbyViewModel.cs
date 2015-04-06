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
        private Lobby _lobby;
        private Profile _userProfile;

        public LobbyViewModel()
        {
            DisconnectCommand = new DelegateCommand(Disconnect);
            EditProfileCommand = new DelegateCommand(EditProfile);
            ViewProfileCommand = new DelegateCommand(ViewProfile);
        }


        public Lobby Lobby
        {
            get { return _lobby; }
            set { SetProperty(ref _lobby, value); }
        }

        public Profile Profile
        {
            get { return _userProfile; }
            set { SetProperty(ref _userProfile, value); }
        }

        public RoomViewModel RoomViewModel
        {
            get { return Container.GetA<RoomViewModel>(); }
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
            Container.GetA<MainViewModel>().NavigateToView(Container.GetA<ProfileViewModel>());
        }

    }
}
